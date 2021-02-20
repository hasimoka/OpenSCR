using OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnvifNetworkCameraClient.Models
{
    public class H264FrameRecorder
    {
        private CancellationTokenSource _cts;

        private Task _frameWriteTask;

        private readonly ConcurrentQueue<RawH264FrameRecordItem> _frameQueue;

        public H264FrameRecorder()
        {
            this._cts = null;
            this._frameWriteTask = null;
            this._frameQueue = new ConcurrentQueue<RawH264FrameRecordItem>();
        }

        public void AddFrame(RawH264FrameRecordItem item)
        {
            this._frameQueue.Enqueue(item);
        }

        public void Start(string outputFolder)
        {
            // 出力先フォルダが存在しなれば、作成する
            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
            }

            if (this._cts == null)
            {
                this._cts = new CancellationTokenSource();

                this._frameWriteTask = Task.Run(async () =>
                {
                    await this.Record(this._cts.Token, this._frameQueue, outputFolder);
                });
            }
        }

        public void Stop()
        {
            if (this._cts != null)
            {
                this._cts.Cancel();
                
                if (!this._frameWriteTask.IsCompleted)
                {
                    Task.Delay(100);
                }
            }
        }

        private async Task Record(CancellationToken token, ConcurrentQueue<RawH264FrameRecordItem> frameQueue, string outputFolder)
        {
            // --
            // 最初のIフレームを取得する
            // --

            RawH264FrameRecordItem frame = null;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    // キャンセルされた場合、ループを終了
                    break;
                }

                if (frameQueue.TryDequeue(out frame))
                {
                    // --
                    // キューからフレームを取得
                    // --

                    if (frame.SpsPpsSegment != null)
                        if (frame.SpsPpsSegment.Count > 0)
                            break;
                }

                // キューにフレームが存在しない、または取得したフレームがIフレームではない
                await Task.Delay(50);
            }

            while (!token.IsCancellationRequested)
            {
                var recordTimeUsingFileName = frame.Timestamp;
                using (var fileStream = new FileStream(Path.Combine(outputFolder, $"{recordTimeUsingFileName:yyyyMMdd_HHmmss}.h264"), FileMode.Create, FileAccess.Write))
                {
                    // 取得済みのIフレームをファイルに保存
                    this.WriteFrame(fileStream, frame);

                    while (!token.IsCancellationRequested)
                    {
                        if (frameQueue.TryDequeue(out frame))
                        {
                            // --
                            // キューからフレームを取得
                            // --

                            if (this.RoundHourDateTime(frame.Timestamp) > this.RoundHourDateTime(recordTimeUsingFileName))
                            {
                                if (frame.SpsPpsSegment != null)
                                    if (frame.SpsPpsSegment.Count > 0)
                                        break;
                            }

                            this.WriteFrame(fileStream, frame);
                        }
                        else
                        {
                            // --
                            // キューにフレームが存在しない
                            // --

                            await Task.Delay(50);
                        }
                    }
                }
            }               
        }

        /// <summary>
        /// 指定されたストリームにフレームを書き込む
        /// </summary>
        /// <param name="h264FrameStream"></param>
        /// <param name="rawH264Frame"></param>
        private void WriteFrame(Stream h264FrameStream, RawH264FrameRecordItem rawH264Frame)
        {
            // タイムスタンプを書き込む
            var timestampBinary = BitConverter.GetBytes(rawH264Frame.Timestamp.ToBinary());
            h264FrameStream.Write(timestampBinary, 0, timestampBinary.Length);

            // 4バイト(int)でフレームサイズを書き込む 
            var frameSize = rawH264Frame.FrameSegment.Count;
            if (rawH264Frame.SpsPpsSegment != null)
            {
                if (rawH264Frame.SpsPpsSegment.Count > 0)
                {
                    frameSize += rawH264Frame.SpsPpsSegment.Count;
                }
            }
            var frameSizeBinary = BitConverter.GetBytes(frameSize);
            h264FrameStream.Write(frameSizeBinary, 0, frameSizeBinary.Length);

            // SpsPpsSegmentを書き込む
            if (rawH264Frame.SpsPpsSegment != null)
            {
                if (rawH264Frame.SpsPpsSegment.Count > 0)
                {
                    h264FrameStream.Write(rawH264Frame.SpsPpsSegment.ToArray(), 0, rawH264Frame.SpsPpsSegment.Count);
                }
            }

            // FrameSegmentを書き込む
            h264FrameStream.Write(rawH264Frame.FrameSegment.ToArray(), 0, rawH264Frame.FrameSegment.Count);
            h264FrameStream.Flush();
        }

        private DateTime RoundHourDateTime(DateTime dateTime)
        {
            var roundedHourDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);

            return roundedHourDateTime;
        }
    }
}
