using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HalationGhost;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CameraClient.Models
{
    public class RawH264Player : BindableModelBase, IDisposable
    {
        private const string RawH264FileFormat = "yyyyMMdd_HHmmss";
        private const string NoImageFilePath = @".\Images\no_image_g.png";
        private const int RequiredBufferSize = 10;

        private readonly OpenH264Lib.Decoder _decoder;

        private readonly LinkedList<string> _rawH264Files;
        private readonly LinkedList<(DateTime Timestamp, BitmapSource Image)> _decodedFrameBuffer;

        private readonly object _bufferLock;

        private readonly LinkedList<(DateTime startFrameTime, DateTime endFrameTime)> _recordedFrameTerms;

        /// <summary>
        /// H264ファイル格納フォルダパス
        /// </summary>
        private string _rawH264Folder;

        private FileStream _rawH264FileStream;

        private LinkedListNode<string> _currentH264FileNode;

        private TimeSpan _timeSpanBetweenPlaytimeAndCurrent;

        private readonly DispatcherTimer _dispatcherTimer;

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public RawH264Player(string rawH264Folder)
        {
            if (!Directory.Exists(rawH264Folder))
            {
                Directory.CreateDirectory(rawH264Folder);
            }
            _rawH264Folder = rawH264Folder;

            // OpenH264デコーダオブジェクトを生成する
            _decoder = new OpenH264Lib.Decoder("openh264-1.7.0-win64.dll");

            // 初期表示用の画像を読み込む
            using (var stream = new MemoryStream(File.ReadAllBytes(NoImageFilePath)))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                NoFrameImage = bitmap;
            }

            FrameImage = new ReactivePropertySlim<BitmapSource>(NoFrameImage)
                .AddTo(Disposable);

            _currentH264FileNode = null;
            _decodedFrameBuffer = new LinkedList<(DateTime Timestamp, BitmapSource Image)>();

            _rawH264FileStream = null;

            _bufferLock = new object();

            // 指定されたフォルダのRawH264ファイル一覧を取得する
            _rawH264Files = GetRawH264Files(rawH264Folder);

            // 指定されたフォルダの録画時間情報を取得する
            _recordedFrameTerms = GetRecordedTerms(rawH264Folder);

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            // 30fps間隔(≒34ms)に設定
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 34);
        }

        public BitmapSource NoFrameImage { get; set; }

        public ReactivePropertySlim<BitmapSource> FrameImage;

        private (DateTime? Timestamp, BitmapSource Image) FirstDecodedFrame
        {
            get
            {
                if (_decodedFrameBuffer.First != null)
                {
                    return _decodedFrameBuffer.First.Value;
                }

                return (null, null);
            }
        }

        public new void Dispose()
        {
            if (_rawH264FileStream != null)
            {
                _rawH264FileStream.Close();
                _rawH264FileStream = null;
            }

            _decoder.Dispose();

            base.Dispose();
        }

        public void Play(DateTime playTime)
        {
            try
            {
                _timeSpanBetweenPlaytimeAndCurrent = DateTime.Now - playTime;

                _currentH264FileNode = FindRawH264File(playTime, _rawH264Files);
                if (_currentH264FileNode == null) return;

                _rawH264FileStream = new FileStream(
                    _currentH264FileNode.Value, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                // 再生時刻直前(再生時刻を含む)のIフレームの位置までFileStreamをSeekする
                if (!SeekPlayTime(_rawH264FileStream, playTime)) _rawH264FileStream.Seek(0, SeekOrigin.Begin);

                _dispatcherTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                _currentH264FileNode = null;
                if (_rawH264FileStream != null)
                {
                    _rawH264FileStream.Close();
                    _rawH264FileStream = null;
                }
            }
        }

        public void Stop()
        {
            _dispatcherTimer.Stop();

            // RawFileStreamを閉じる
            if (_rawH264FileStream != null)
            {
                _rawH264FileStream.Close();
                _rawH264FileStream = null;
            }

            _currentH264FileNode = null;

            // フレームバッファをクリアする
            _decodedFrameBuffer.Clear();
        }

        public LinkedList<(DateTime startFrameTime, DateTime endFrameTime)> SearchRecordedTerms(DateTime searchStartTime, DateTime searchEndTime)
        {
            var recordedTerms = new LinkedList<(DateTime startFrameTime, DateTime endFrameTime)>();

            foreach (var (recordedFrameStartTime, recordedFrameEndTime) in _recordedFrameTerms)
            {
                if (recordedFrameEndTime < searchStartTime) continue;
                if (searchEndTime < recordedFrameStartTime) continue;


                DateTime startTime = searchStartTime < recordedFrameStartTime ? recordedFrameStartTime : searchStartTime;
                DateTime endTime = recordedFrameEndTime < searchEndTime ? recordedFrameEndTime : searchEndTime;

                recordedTerms.AddLast((startTime, endTime));
            }

            return recordedTerms;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // バッファにフレーム読み込みをおこなう
            PushDecodedFrameToBufferAsync();

            // 再生表示日時を算出する
            var playingTime = DateTime.Now - this._timeSpanBetweenPlaytimeAndCurrent;

            var isContinue = true;
            do
            {
                if (FirstDecodedFrame.Timestamp != null)
                {
                    if (FirstDecodedFrame.Timestamp.Value <= playingTime)
                    {
                        var decodedFrameItem = PollFirstDecodedFrame();
                        FrameImage.Value = decodedFrameItem.Image;
                    }
                    else if ((playingTime - FirstDecodedFrame.Timestamp.Value).Duration() >=
                             new TimeSpan(0, 0, 30))
                    {
                        // 表示時刻とバッファに格納されたフレーム時刻に30秒以上の差異がある場合
                        FrameImage.Value = NoFrameImage;
                        isContinue = false;
                    }
                    else
                    {
                        isContinue = false;
                    }
                }
                else
                {
                    // バッファが空の場合
                    FrameImage.Value = NoFrameImage;
                    isContinue = false;
                }
            } while (isContinue);
        }

        private (DateTime? Timestamp, BitmapSource Image) PollFirstDecodedFrame()
        {
            if (_decodedFrameBuffer.First != null)
            {
                var firstItem = _decodedFrameBuffer.First.Value;
                _decodedFrameBuffer.RemoveFirst();

                return firstItem;
            }

            return (null, null);
        }

        /// <summary>
        /// 指定されたフォルダのh264ファイル名一覧を取得します。
        /// </summary>
        /// <param name="rawH264Folder">h264ファイルが格納されているフォルダパス。</param>
        /// <returns>見つかったh264ファイルパスを格納するコレクション。</returns>
        private static LinkedList<string> GetRawH264Files(string rawH264Folder)
        {
            var rawH264Files = new LinkedList<string>();

            // "Movies"フォルダのh264ファイルをファイル名順で取得する
            var files = Directory.GetFiles(rawH264Folder, "*.h264");
            foreach (var file in files.OrderBy(x => x))
            {
                rawH264Files.AddLast(Path.GetFullPath(file));
            }

            return rawH264Files;
        }

        private static bool FindIFrameFromBuffer(byte[] rawH264FrameBuffer)
        {
            var isFound = false;

            var startPos = 0;
            do
            {
                // デコードに使用するフレームをバッファから取得する
                var findTargetSpan = new Span<byte>(rawH264FrameBuffer, startPos, rawH264FrameBuffer.Length - startPos);
                var nalUnitSize = FindNalUnit(findTargetSpan, out var nalStart, out var nalEnd);
                if (nalUnitSize != 0)
                {
                    // NALユニット分のSpanを切り出す
                    var nalUnitSpan = new Span<byte>(findTargetSpan.ToArray(), 0, nalEnd);

                    switch (nalUnitSpan[nalStart] & 0x1F)
                    {
                        case 6: // nalUnitType = "SEI";
                        case 7: // nalUnitType = "SPS";
                        case 8: // nalUnitType = "PPS";
                        case 9: // nalUnitType = "AUD";
                            {
                                isFound = true;
                            }
                            break;
                        default:    // nalUnitType = "non-IDR picture", "IDR picture", ...
                            break;
                    }

                    // フレームバッファの読み込み位置を更新する
                    startPos += nalEnd;
                }
                else
                {
                    break;
                }
            } while (!isFound);

            return isFound;
        }

        private static int FindNalUnit(Span<byte> buffer, out int nalStart, out int nalEnd)
        {
            // find start
            nalStart = 0;
            nalEnd = 0;

            if (buffer.Length == 0)
                return 0;

            var i = 0;
            while ( // ( next_bits( 24 ) != 0x000001 && next_bits( 32 ) != 0x00000001 )
                (buffer[i] != 0 || buffer[i + 1] != 0 || buffer[i + 2] != 0x01) &&
                (buffer[i] != 0 || buffer[i + 1] != 0 || buffer[i + 2] != 0 || buffer[i + 3] != 0x01))
            {
                i++; // skip leading zero
                if (i + 4 >= buffer.Length) { return 0; } // did not find nal start
            }

            if (buffer[i] != 0 || buffer[i + 1] != 0 || buffer[i + 2] != 0x01) // ( next_bits( 24 ) != 0x000001 )
            {
                i++;
            }

            if (buffer[i] != 0 || buffer[i + 1] != 0 || buffer[i + 2] != 0x01) { /* error, should never happen */ return 0; }
            i += 3;

            nalStart = i;

            while (   //( next_bits( 24 ) != 0x000000 && next_bits( 24 ) != 0x000001 )
                (buffer[i] != 0 || buffer[i + 1] != 0 || buffer[i + 2] != 0) &&
                (buffer[i] != 0 || buffer[i + 1] != 0 || buffer[i + 2] != 0x01)
                )
            {
                i++;
                // FIXME the next line fails when reading a nal that ends exactly at the end of the data
                if (i + 3 >= buffer.Length)
                {
                    nalEnd = buffer.Length;
                    return -1;
                } // did not find nal end, stream ended first
            }

            nalEnd = i;

            return (nalEnd - nalStart);
        }

        private LinkedList<(DateTime startFrameTime, DateTime endFrameTime)> GetRecordedTerms(string rawH264Folder)
        {
            var recordedTerms = new LinkedList<(DateTime startFrameTime, DateTime endFrameTime)>();

            if (!Directory.Exists(rawH264Folder)) return recordedTerms;

            var rawH264Files = GetRawH264Files(rawH264Folder);

            // ファイルの最初のフレームと最後のフレームの時刻を取得する

            foreach(var rawH264FilePath in rawH264Files)
            {
                var (startTime, endTime) = GetStartAndEndTimeInRawH264File(rawH264FilePath);

                if (!startTime.HasValue || !endTime.HasValue)
                    continue;

                if (recordedTerms.Count == 0)
                {
                    recordedTerms.AddLast((startTime.Value, endTime.Value));
                }
                else
                {
                    var lastFrameEndTime = recordedTerms.Last.Value.endFrameTime;
                    if (startTime - lastFrameEndTime <= TimeSpan.FromSeconds(1.0d))
                    {
                        // 最後のフレーム時刻との時刻差が1秒以内であれば継続していると判断する
                        var lastFrameStartTime = recordedTerms.Last.Value.startFrameTime;
                        recordedTerms.Last.Value = (lastFrameStartTime, endTime.Value);
                    }
                    else
                    {
                        recordedTerms.AddLast((startTime.Value, endTime.Value));
                    }
                }
            }

            return recordedTerms;
        }

        private void PushDecodedFrameToBufferAsync()
        {
            // H264ファイルの読み込みをおこない、H264フレームをデコードし、バッファに格納する
            if (_decodedFrameBuffer.Count <= RequiredBufferSize)
            {
                var isFileEnd = GetDecodedRawH264Frame();
                if (isFileEnd)
                {
                    // ファイルの終端まで読み込んだので、H264FileStreamを閉じる
                    if (_rawH264FileStream != null)
                    {
                        _rawH264FileStream.Close();
                        _rawH264FileStream = null;
                    }

                    // h264FileStreamに次のファイルストリームをセットする
                    if (_currentH264FileNode.Next != null)
                    {
                        _currentH264FileNode = _currentH264FileNode.Next;
                        var newH264FileName = _currentH264FileNode.Value;
                        _rawH264FileStream = new FileStream(newH264FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    }
                }
            }
        }

        private bool GetDecodedRawH264Frame()
        {
            if (_rawH264FileStream == null)
                return true;

            try
            {
                // ファイルから12バイト読み込む
                var timestampBuffer = new byte[12];
                var readSize = _rawH264FileStream.Read(timestampBuffer, 0, timestampBuffer.Length);

                if (readSize <= 0)
                {
                    // ファイルの終端まで読み込んだと判断する
                    return true;
                }
                else if (readSize != 12)
                {
                    return false;
                }

                var timestamp = new DateTime(BitConverter.ToInt64(timestampBuffer, 0));
                var frameSize = BitConverter.ToInt32(timestampBuffer, 8);

                var frameBuffer = new byte[frameSize];
                readSize = _rawH264FileStream.Read(frameBuffer, 0, frameBuffer.Length);
                if (readSize > 0)
                {
                    var bitmap = _decoder.Decode(frameBuffer, frameBuffer.Length);
                    if (bitmap != null)
                    {
                        // HBitmapに変換
                        var hBitmap = bitmap.GetHbitmap();

                        // HBitmapからBitmapSourceを作成
                        try
                        {
                            var bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                                hBitmap,
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions()
                            );
                            bitmapSource.Freeze();

                            lock (_bufferLock)
                            {
                                _decodedFrameBuffer.AddLast((timestamp, bitmapSource));
                            }

                        }
                        finally
                        {
                            DeleteObject(hBitmap);
                        }
                    }
                }
                else
                {
                    // ファイルの終端まで読み込んだと判断する
                    return true;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        /// <summary>
        /// 指定された時刻を含むRawH264ファイルを探します。
        /// </summary>
        /// <param name="targetTime"></param>
        /// <param name="rawH264Files"></param>
        /// <returns></returns>
        private LinkedListNode<string> FindRawH264File(DateTime targetTime, LinkedList<string> rawH264Files)
        {
            LinkedListNode<string> resultNode = null;

            var currentNode = rawH264Files.First;
            while(currentNode != null)
            {
                var (startTime, endTime) = GetStartAndEndTimeInRawH264File(currentNode.Value);
                if (currentNode == rawH264Files.First)
                {
                    // ファイルリストの最初のファイルの場合で、対象時刻がファイルの開始時刻以前の場合は最初のファイルを選択する
                    if (targetTime < startTime)
                    {
                        resultNode = currentNode;
                        break;
                    }
                }

                if (currentNode.Next != null)
                {
                    // 次のRawH264ファイルがある場合
                    var nextFileTime = GetStartAndEndTimeInRawH264File(currentNode.Next.Value);

                    if (startTime <= targetTime && targetTime < nextFileTime.StartTime)
                    {
                        var startAndEndTime = GetStartAndEndTimeInRawH264File(currentNode.Value);
                        if (startAndEndTime.StartTime <= targetTime && targetTime <= startAndEndTime.EndTime)
                        {
                            resultNode = currentNode;
                            break;
                        }
                    }
                }
                else
                {
                    // 次のRawH264ファイルがない場合
                    if (startTime <= targetTime && targetTime <= endTime)
                    {
                        resultNode = currentNode;
                        break;
                    }
                }

                currentNode = currentNode.Next;
            }

            return resultNode;
        }

        private (DateTime? StartTime, DateTime? EndTime) GetStartAndEndTimeInRawH264File(string rawH264File)
        {
            (DateTime? StartTime, DateTime? EndTime) result = (null, null);

            try
            {
                using(var rawH264FileStream = new FileStream(rawH264File, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    while (true)
                    {
                        // ファイルから12バイト読み込む
                        var timestampBuffer = new byte[12];
                        var readSize = rawH264FileStream.Read(timestampBuffer, 0, timestampBuffer.Length);

                        if (readSize != 12)
                            break;

                        var fileStreamPos = rawH264FileStream.Position;

                        var timestamp = new DateTime(BitConverter.ToInt64(timestampBuffer, 0));
                        if (result.StartTime == null)
                        {
                            result.StartTime = timestamp;
                        }
                        result.EndTime = timestamp;

                        // フレームサイズ分Seekして、ストリームを次のヘッダ位置まで移動する
                        var frameSize = BitConverter.ToInt32(timestampBuffer, 8);
                        fileStreamPos += frameSize;
                        var seekPos = rawH264FileStream.Seek(frameSize, SeekOrigin.Current);
                        if (seekPos != fileStreamPos)
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        private bool SeekPlayTime(Stream fileStream, DateTime playTime)
        {
            if (!SeekIFrameBeforePlayTime(fileStream, playTime))
                return false;

            var isSucceededSeek = false;
            byte[] preFrameBuffer = null;
            var preFramePos = 0L;
            while (true)
            {
                var currentFramePos = _rawH264FileStream.Position;

                // ファイルから12バイト読み込む
                var frameTimestampBuffer = new byte[12];
                var readHeaderSize = _rawH264FileStream.Read(frameTimestampBuffer, 0, frameTimestampBuffer.Length);

                if (readHeaderSize != 12)
                    break;

                var frameTimestamp = new DateTime(BitConverter.ToInt64(frameTimestampBuffer, 0));
                if (frameTimestamp < playTime)
                {
                    // 再生時刻より前(再生時刻を含まない)のフレームの場合
                    var frameSize = BitConverter.ToInt32(frameTimestampBuffer, 8);
                    var frameBuffer = new byte[frameSize];
                    var readFrameSize = _rawH264FileStream.Read(frameBuffer, 0, frameBuffer.Length);
                    if (readFrameSize > 0)
                    {
                        if (preFrameBuffer != null)
                        {
                            // 読み込んだフレームバッファをデコーダに投入する(IFrame以降のフレームが必要となるため)
                            _decoder.Decode(preFrameBuffer, preFrameBuffer.Length);

                            preFramePos = currentFramePos;
                        }

                        preFrameBuffer = frameBuffer;
                    }
                    else
                    {
                        // ファイルの終端まで読み込んだと判断する
                        break;
                    }
                }
                else
                {
                    // 再生時刻以降(再生時刻を含む)のフレームが見つかった
                    fileStream.Seek(preFramePos, SeekOrigin.Begin);
                    isSucceededSeek = true;
                    break;
                }
            }

            return isSucceededSeek;
        }

        private bool SeekIFrameBeforePlayTime(Stream fileStream, DateTime playTime)
        {
            // ファイルの先頭にSeekする
            fileStream.Seek(0, SeekOrigin.Begin);

            var preIFramePos = -1L;
            var preFramePos = 0L;
            while (true)
            {
                var currentFramePos = _rawH264FileStream.Position;

                // ファイルから12バイト読み込む
                var timestampBuffer = new byte[12];
                var readHeaderSize = _rawH264FileStream.Read(timestampBuffer, 0, timestampBuffer.Length);

                if (readHeaderSize != 12)
                    break;

                var frameTimestamp = new DateTime(BitConverter.ToInt64(timestampBuffer, 0));
                var frameSize = BitConverter.ToInt32(timestampBuffer, 8);
                var frameBuffer = new byte[frameSize];

                if (frameTimestamp <= playTime)
                {
                    var readFrameSize = _rawH264FileStream.Read(frameBuffer, 0, frameBuffer.Length);
                    if (readFrameSize > 0)
                    {
                        if (FindIFrameFromBuffer(frameBuffer))
                        {
                            preIFramePos = preFramePos;
                        }

                        preFramePos = currentFramePos;
                    }
                    else
                    {
                        // ファイルの終端まで読み込んだと判断する
                        break;
                    }
                }
                else
                {
                    // フレーム時刻が再生時刻を超えた
                    break;
                }
            }

            if (preIFramePos >= 0)
            {
                fileStream.Seek(preIFramePos, SeekOrigin.Begin);
            }
            else
            {
                fileStream.Seek(0, SeekOrigin.Begin);
            }

            return preIFramePos >= 0 ? true : false;
        }
    }
}
