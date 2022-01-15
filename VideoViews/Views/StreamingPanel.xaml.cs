using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Events;
using VideoViews.ViewModels;

namespace VideoViews.Views
{
    /// <summary>
    /// StreamingPanel.xaml の相互作用ロジック
    /// </summary>
    public partial class StreamingPanel : UserControl
    {
        private readonly LinkedList<(DateTime? lineTime, Line lineObject)> _timeLineControls;
        private readonly LinkedList<TextBlock> _timeTextBlocks;
        private readonly LinkedList<Polygon> _recordedTermRectangles;

        private DateTime? _preDrawnTime;

        /// <summary>
        /// Timelineキャンバス上でのマウス押下中フラグ
        /// </summary>
        private bool _isMouseDownOnTimelineCanvas;

        /// <summary>
        /// Timelineキャンバス上でマウス移動開始座標
        /// </summary>
        private Point _mouseStartPointOnTimelineCanvas;

        private Point _preMousePointOnTimelineCanvas;

        public StreamingPanel()
        {
            InitializeComponent();

            _timeLineControls = new LinkedList<(DateTime? lineTime, Line lineObject)>();
            _timeTextBlocks = new LinkedList<TextBlock>();
            _recordedTermRectangles = new LinkedList<Polygon>();

            _isMouseDownOnTimelineCanvas = false;
            _mouseStartPointOnTimelineCanvas = new Point();
            _preMousePointOnTimelineCanvas = new Point();

            _preDrawnTime = null;

            Messenger.Instance.GetEvent<PubSubEvent<DateTime>>()
                .Subscribe(playTime => {
                    if (_preDrawnTime.HasValue)
                    {
                        if (Truncate(playTime, TimeSpan.FromSeconds(1)) != Truncate(_preDrawnTime.Value, TimeSpan.FromSeconds(1)))
                            DrawTimeLine(playTime);
                    }
                    else
                    {
                        DrawTimeLine(playTime);
                    }
                });
        }

        private StreamingPanelViewModel ViewModel => this.DataContext as StreamingPanelViewModel;

        private void TimelineCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseStartPointOnTimelineCanvas = e.GetPosition(TimelineCanvas);
            _preMousePointOnTimelineCanvas = _mouseStartPointOnTimelineCanvas;
            _isMouseDownOnTimelineCanvas = true;

            // イベントを処理済みとする（当イベントがこの先伝搬されるのを止めるため）
            e.Handled = true;
        }

        private void TimelineCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDownOnTimelineCanvas)
                return;

            var currentPos = e.GetPosition(TimelineCanvas);

            // マウスの移動量(Pixel)から時間移動量(秒)を計算する
            var movePixels = (int)Math.Ceiling(Math.Abs(currentPos.X - this._preMousePointOnTimelineCanvas.X));
            if (_preMousePointOnTimelineCanvas.X < currentPos.X)
                movePixels *= -1;

            // TimeLineをマウスの移動量分だけ移動する
            MoveTimeLine(movePixels);

            _preMousePointOnTimelineCanvas = currentPos;

            // イベントを処理済みとする（当イベントがこの先伝搬されるのを止めるため）
            e.Handled = true;
        }

        private void TimelineCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseDownOnTimelineCanvas)
            {
                // ViewModelに時間移動を依頼する
                if (_preDrawnTime.HasValue)
                    ViewModel.MoveDisplayTime(_preDrawnTime.Value);

                // イベントを処理済みとする（当イベントがこの先伝搬されるのを止めるため）
                e.Handled = true;
            }

            _isMouseDownOnTimelineCanvas = false;
        }

        private void TimelineCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isMouseDownOnTimelineCanvas)
            {
                // ViewModelに時間移動を依頼する
                if (_preDrawnTime.HasValue)
                    ViewModel.MoveDisplayTime(_preDrawnTime.Value);

                // イベントを処理済みとする（当イベントがこの先伝搬されるのを止めるため）
                e.Handled = true;
            }

            _isMouseDownOnTimelineCanvas = false;
        }

        private void DrawTimeLine(DateTime playTime)
        {
            if (_isMouseDownOnTimelineCanvas)
                return;

            BaseLineTop.X1 = TimelineCanvas.Margin.Left;
            BaseLineTop.X2 = TimelineCanvas.ActualWidth - (TimelineCanvas.Margin.Left + TimelineCanvas.Margin.Right);
            BaseLineTop.Y1 = 5.0d;
            BaseLineTop.Y2 = 5.0d;

            BaseLineCenter.X1 = TimelineCanvas.Margin.Left;
            BaseLineCenter.X2 = TimelineCanvas.ActualWidth - (TimelineCanvas.Margin.Left + TimelineCanvas.Margin.Right);
            BaseLineCenter.Y1 = 35.0d;
            BaseLineCenter.Y2 = 35.0d;

            BaseLineBottom.X1 = TimelineCanvas.Margin.Left;
            BaseLineBottom.X2 = TimelineCanvas.ActualWidth - (TimelineCanvas.Margin.Left + TimelineCanvas.Margin.Right);
            BaseLineBottom.Y1 = 60.0d;
            BaseLineBottom.Y2 = 60.0d;

            //--
            // 時刻線表示処理
            //--

            // 1Pixelあたりの秒数を計算する
            var secondsPerPixel = (int)Math.Ceiling(3600.0d / TimelineCanvas.ActualWidth);
            // 1分あたりのPixel数を計算する
            var pixelsPerOneMinute = (int)Math.Ceiling(60.0d / secondsPerPixel);
            var minutesLineControls = new LinkedList<(DateTime PosTime, int XPos)>();

            // 録画済み時間帯の描画をおこなう
            var playTimeXPos = (int)Math.Ceiling(TimelineCanvas.ActualWidth / 2.0d);
            var firstTime = playTime - TimeSpan.FromSeconds(playTimeXPos * secondsPerPixel);
            var lastTime = playTime + TimeSpan.FromSeconds((TimelineCanvas.ActualWidth - playTimeXPos) * secondsPerPixel);

            foreach (var recordedTermRectangle in _recordedTermRectangles)
            {
                TimelineCanvas.Children.Remove(recordedTermRectangle);
            }
            _recordedTermRectangles.Clear();

            var recordedTerms = ViewModel.SearchRecordedTerm(firstTime, lastTime);
            foreach (var (startFrameTime, endFrameTime) in recordedTerms)
            {
                // 描画開始位置を計算する
                var timeLagForStartFrameTime = playTime - startFrameTime;
                var startFrameTimeXPos = playTimeXPos - (int)Math.Ceiling((decimal)timeLagForStartFrameTime.TotalSeconds / (decimal)secondsPerPixel);
                if (startFrameTimeXPos < 0)
                    startFrameTimeXPos = 0;

                // 描画終了位置を計算する
                var timeLagForEndFrameTime = endFrameTime - playTime;
                var endFrameTimeXPos = playTimeXPos + (int)Math.Ceiling((decimal)timeLagForEndFrameTime.TotalSeconds / (decimal)secondsPerPixel);
                if (this.TimelineCanvas.ActualWidth < endFrameTimeXPos)
                    endFrameTimeXPos = (int)this.TimelineCanvas.ActualWidth;

                var recordedTermRectangle = new Polygon()
                {
                    Stroke = new SolidColorBrush(Colors.Chartreuse),
                    Fill = new SolidColorBrush(Colors.Chartreuse),
                    FillRule = FillRule.Nonzero,
                    IsHitTestVisible = false
                };
                recordedTermRectangle.Points.Add(new Point(startFrameTimeXPos, 20.0d));
                recordedTermRectangle.Points.Add(new Point(endFrameTimeXPos, 20.0d));
                recordedTermRectangle.Points.Add(new Point(endFrameTimeXPos, 33.0d));
                recordedTermRectangle.Points.Add(new Point(startFrameTimeXPos, 33.0d));
                Canvas.SetZIndex(recordedTermRectangle, 0);
                TimelineCanvas.Children.Add(recordedTermRectangle);
                _recordedTermRectangles.AddLast(recordedTermRectangle);
            }

            // 表示時刻の時刻線を追加する
            var moveAmount = (int)Math.Ceiling((decimal)playTime.Second / (decimal)secondsPerPixel);
            var centerMinutesTime = Truncate(playTime, TimeSpan.FromMinutes(1.0d));
            var centerMinutesPos = (int)Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d) - moveAmount;
            minutesLineControls.AddLast((centerMinutesTime, centerMinutesPos));
            Console.WriteLine($"PlayTime={playTime:HH:mm:ss}, centerMinutesTime={centerMinutesTime:HH:mm:ss}, XPos={centerMinutesPos - moveAmount}");

            // 現在時刻より前の時刻線位置を計算する
            var calTime = centerMinutesTime;
            var calPos = centerMinutesPos;
            do
            {
                calTime -= TimeSpan.FromMinutes(1.0d);
                calPos -= pixelsPerOneMinute;

                if (calPos < (int)this.TimelineCanvas.Margin.Left)
                    break;

                minutesLineControls.AddFirst((calTime, calPos));
            } while (true);

            // 現在時刻より後の時刻線位置を計算する
            calTime = centerMinutesTime;
            calPos = centerMinutesPos;
            do
            {
                calTime += TimeSpan.FromMinutes(1.0d);
                calPos += pixelsPerOneMinute;
                if ((int)(this.TimelineCanvas.ActualWidth - (this.TimelineCanvas.Margin.Left + this.TimelineCanvas.Margin.Right)) < calPos)
                    break;

                minutesLineControls.AddLast((calTime, calPos));
            } while (true);

            // 時刻ラベルを削除する
            foreach (var labelControl in this._timeTextBlocks)
            {
                this.TimelineCanvas.Children.Remove(labelControl);
            }
            this._timeTextBlocks.Clear();

            // 時刻線の描画をおこなう
            var currentLinkedListPos = minutesLineControls.First;
            if (this._timeLineControls.Count < minutesLineControls.Count)
            {
                var addCount = minutesLineControls.Count - this._timeLineControls.Count;
                for (int i = 0; i < addCount; i++)
                {
                    var lineObject = new Line
                    {
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 1.0d,
                        IsHitTestVisible = false
                    };

                    Canvas.SetZIndex(lineObject, 1);
                    this.TimelineCanvas.Children.Add(lineObject);
                    this._timeLineControls.AddLast((null, lineObject));
                }
            }
            else if (minutesLineControls.Count < this._timeLineControls.Count)
            {
                var minusCount = this._timeLineControls.Count - minutesLineControls.Count;
                for (var i = 0; i < minusCount; i++)
                {
                    var lastTimeLineControl = this._timeLineControls.Last;
                    if (lastTimeLineControl != null)
                    {
                        this.TimelineCanvas.Children.Remove(lastTimeLineControl.Value.lineObject);
                        this._timeLineControls.RemoveLast();
                    }
                }
            }

            var lineControl = this._timeLineControls.First;
            while (lineControl != null)
            {
                var lineObject = lineControl.Value.lineObject;

                if (currentLinkedListPos != null)
                {
                    lineObject.Visibility = Visibility.Visible;

                    lineObject.X1 = currentLinkedListPos.Value.XPos;
                    lineObject.X2 = currentLinkedListPos.Value.XPos;
                    lineObject.Y1 = 35.0d;
                    if (RoundUp(currentLinkedListPos.Value.PosTime, TimeSpan.FromMinutes(1.0d)).Minute % 5 == 0)
                    {
                        // 5分毎の時刻線
                        lineObject.Y2 = 45.0d;
                    }
                    else
                    {
                        // 1分毎の時刻線
                        lineObject.Y2 = 41.0d;
                    }

                    if (RoundUp(currentLinkedListPos.Value.PosTime, TimeSpan.FromMinutes(1.0d)).Minute % 10 == 0)
                    {
                        var timeTextBlock = new TextBlock
                        {
                            FontSize = 10.0d,
                            Text = $"{currentLinkedListPos.Value.PosTime:HH:mm}",
                            IsHitTestVisible = false
                        };

                        // 文字サイズを計算する
                        var formattedText = new FormattedText(
                            timeTextBlock.Text,
                            System.Globalization.CultureInfo.CurrentCulture,
                            FlowDirection.LeftToRight,
                            new Typeface(timeTextBlock.FontFamily, timeTextBlock.FontStyle, timeTextBlock.FontWeight, timeTextBlock.FontStretch),
                            timeTextBlock.FontSize,
                            Brushes.Black,
                            VisualTreeHelper.GetDpi(this).PixelsPerDip);

                        Canvas.SetTop(timeTextBlock, 45.0d);
                        Canvas.SetLeft(timeTextBlock, Math.Ceiling(currentLinkedListPos.Value.XPos - ((timeTextBlock.ActualWidth / 2.0d) + (formattedText.Width / 2.0d))));
                        Canvas.SetZIndex(timeTextBlock, 2);
                        this.TimelineCanvas.Children.Add(timeTextBlock);
                        this._timeTextBlocks.AddLast(timeTextBlock);
                    }

                    lineControl.Value = (currentLinkedListPos.Value.PosTime, lineObject);
                    currentLinkedListPos = currentLinkedListPos.Next;
                }
                else
                {
                    lineObject.Visibility = Visibility.Hidden;

                    lineControl.Value = (null, lineObject);
                }

                lineControl = lineControl.Next;
            }

            // --
            // 現在時刻マーカー
            // --

            this.TimelineCanvas.Children.Remove(this.CenterLine);
            this.CenterLine = new Line
            {
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 2.0d,
                X1 = Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d),
                X2 = Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d),
                Y1 = 6.0d,
                Y2 = 33.0d,
                IsHitTestVisible = false
            };
            Canvas.SetZIndex(CenterLine, 3);
            this.TimelineCanvas.Children.Add(this.CenterLine);

            this.TimelineCanvas.Children.Remove(this.CenterLineTriangle);
            this.CenterLineTriangle = new Polygon
            {
                Stroke = new SolidColorBrush(Colors.Red),
                Fill = new SolidColorBrush(Colors.Red),
                FillRule = FillRule.Nonzero,
                IsHitTestVisible = false
            };
            this.CenterLineTriangle.Points.Add(new Point(Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d), 28.0d));
            this.CenterLineTriangle.Points.Add(new Point(Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d + 5.0d), 33.0d));
            this.CenterLineTriangle.Points.Add(new Point(Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d - 5.0d), 33.0d));
            Canvas.SetZIndex(CenterLineTriangle, 3);
            this.TimelineCanvas.Children.Add(this.CenterLineTriangle);

            //--
            // 表示時刻を更新する
            //--

            this.DisplayDateTextBlock.Text = $"{playTime:MM月dd日 (ddd)}";
            this.DisplayTimeTextBlock.Text = $"{playTime:HH:mm:ss}";

            this._preDrawnTime = playTime;
        }

        private void MoveTimeLine(int movePixels)
        {
            //--
            // 時刻線表示処理
            //--

            // 1Pixelあたりの秒数を計算する
            var secondsPerPixel = (int)Math.Ceiling(3600.0d / this.TimelineCanvas.ActualWidth);

            // 一分あたりのPixel数を計算する
            var oneMinutePixels = (int)Math.Ceiling(60.0d / secondsPerPixel);

            // 保持している時間軸情報とcanvasの幅から最初と最後の時間軸を算出する
            if (this._timeLineControls.First != null)
            {
                var (firstLineTime, firstLineObject) = this._timeLineControls.First.Value;
                if (firstLineTime.HasValue)
                {
                    var movedFirstLineXPos = (int)Math.Ceiling(firstLineObject.X1 - movePixels);
                    if (movedFirstLineXPos < this.TimelineCanvas.Margin.Left)
                    {
                        this.TimelineCanvas.Children.Remove(firstLineObject);
                        this._timeLineControls.RemoveFirst();
                    }

                    if ((int)Math.Ceiling(this.TimelineCanvas.Margin.Left) <= (movedFirstLineXPos - oneMinutePixels))
                    {
                        var newLineTime = firstLineTime.Value - TimeSpan.FromMinutes(1.0d);
                        var newLineObject = new Line
                        {
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeThickness = 1.0d,
                            X1 = firstLineObject.X1 - oneMinutePixels,
                            X2 = firstLineObject.X1 - oneMinutePixels,
                            Y1 = 35.0d,
                            IsHitTestVisible = false
                        };
                        if (RoundUp(newLineTime, TimeSpan.FromMinutes(1.0d)).Minute % 5 == 0)
                        {
                            // 5分毎の時刻線
                            newLineObject.Y2 = 45.0d;
                        }
                        else
                        {
                            // 1分毎の時刻線
                            newLineObject.Y2 = 41.0d;
                        }

                        Canvas.SetZIndex(newLineObject, 1);
                        this.TimelineCanvas.Children.Add(newLineObject);
                        this._timeLineControls.AddFirst((newLineTime, newLineObject));
                    }
                }
            }

            if (this._timeLineControls.Last != null)
            {
                var (lastLineTime, lastLineObject) = this._timeLineControls.Last.Value;
                if (lastLineTime.HasValue)
                {
                    var movedLastLineXPos = (int)Math.Ceiling(lastLineObject.X1 - movePixels);
                    if ((int)Math.Ceiling(this.TimelineCanvas.ActualWidth -
                                           (this.TimelineCanvas.Margin.Left + this.TimelineCanvas.Margin.Right)) <
                        movedLastLineXPos)
                    {
                        this.TimelineCanvas.Children.Remove(lastLineObject);
                        this._timeLineControls.RemoveLast();
                    }

                    if ((movedLastLineXPos + oneMinutePixels) <= (int)Math.Ceiling(this.TimelineCanvas.ActualWidth -
                        (this.TimelineCanvas.Margin.Left + this.TimelineCanvas.Margin.Right)))
                    {
                        var newLineTime = lastLineTime.Value + TimeSpan.FromMinutes(1.0d);
                        var newLineObject = new Line
                        {
                            Stroke = new SolidColorBrush(Colors.Black),
                            StrokeThickness = 1.0d,
                            X1 = lastLineObject.X1 + oneMinutePixels,
                            X2 = lastLineObject.X1 + oneMinutePixels,
                            Y1 = 35.0d,
                            IsHitTestVisible = false
                        };
                        if (RoundUp(newLineTime, TimeSpan.FromMinutes(1.0d)).Minute % 5 == 0)
                        {
                            // 5分毎の時刻線
                            newLineObject.Y2 = 45.0d;
                        }
                        else
                        {
                            // 1分毎の時刻線
                            newLineObject.Y2 = 41.0d;
                        }

                        Canvas.SetZIndex(newLineObject, 1);
                        this.TimelineCanvas.Children.Add(newLineObject);
                        this._timeLineControls.AddLast((newLineTime, newLineObject));
                    }
                }
            }

            // 時刻ラベルを削除する
            foreach (var labelControl in this._timeTextBlocks)
            {
                this.TimelineCanvas.Children.Remove(labelControl);
            }
            this._timeTextBlocks.Clear();

            // 時刻線の描画をおこなう
            (DateTime? lineTime, Line lineObj) closestLine = (null, null);
            foreach (var (lineTime, lineObject) in this._timeLineControls)
            {
                if (!lineTime.HasValue)
                    continue;

                lineObject.X1 = Math.Ceiling(lineObject.X1 - movePixels);
                lineObject.X2 = Math.Ceiling(lineObject.X2 - movePixels);

                if (RoundUp(lineTime.Value, TimeSpan.FromMinutes(1.0d)).Minute % 10 == 0)
                {
                    var timeTextBlock = new TextBlock
                    {
                        FontSize = 10.0d,
                        Text = $"{lineTime.Value:HH:mm}",
                        IsHitTestVisible = false
                    };

                    // 文字サイズを計算する
                    var formattedText = new FormattedText(
                        timeTextBlock.Text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(timeTextBlock.FontFamily, timeTextBlock.FontStyle, timeTextBlock.FontWeight, timeTextBlock.FontStretch),
                        timeTextBlock.FontSize,
                        Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    Canvas.SetTop(timeTextBlock, 45.0d);
                    Canvas.SetLeft(timeTextBlock, Math.Ceiling(lineObject.X1 - ((timeTextBlock.ActualWidth / 2.0d) + (formattedText.Width / 2.0d))));

                    Canvas.SetZIndex(timeTextBlock, 2);
                    this.TimelineCanvas.Children.Add(timeTextBlock);
                    this._timeTextBlocks.AddLast(timeTextBlock);
                }

                if (this.TimelineCanvas.Margin.Left <= lineObject.X1 && lineObject.X1 <= (int)Math.Ceiling(this.TimelineCanvas.ActualWidth - (this.TimelineCanvas.Margin.Left + this.TimelineCanvas.Margin.Right)))
                {
                    lineObject.Visibility = Visibility.Visible;
                }
                else
                {
                    lineObject.Visibility = Visibility.Hidden;
                }

                if (lineObject.X1 < (int)Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d))
                {
                    closestLine = (lineTime.Value, lineObject);
                }
            }

            // --
            // 現在時刻マーカー
            // --

            this.TimelineCanvas.Children.Remove(this.CenterLine);
            this.TimelineCanvas.Children.Remove(this.CenterLineTriangle);

            this.CenterLine = new Line
            {
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 2.0d,
                X1 = Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d),
                X2 = Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d),
                Y1 = 6.0d,
                Y2 = 33.0d,
                IsHitTestVisible = false
            };
            Canvas.SetZIndex(CenterLine, 3);
            this.TimelineCanvas.Children.Add(this.CenterLine);

            this.CenterLineTriangle = new Polygon
            {
                Stroke = new SolidColorBrush(Colors.Red),
                Fill = new SolidColorBrush(Colors.Red),
                FillRule = FillRule.Nonzero,
                IsHitTestVisible = false
            };
            this.CenterLineTriangle.Points.Add(new Point(Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d), 28.0d));
            this.CenterLineTriangle.Points.Add(new Point(Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d + 5.0d), 33.0d));
            this.CenterLineTriangle.Points.Add(new Point(Math.Ceiling(this.TimelineCanvas.ActualWidth / 2.0d - 5.0d), 33.0d));
            Canvas.SetZIndex(CenterLineTriangle, 3);
            this.TimelineCanvas.Children.Add(this.CenterLineTriangle);

            // --
            // 表示時刻を更新する
            // --

            if (closestLine.lineTime.HasValue)
            {
                var additionSeconds = (int)Math.Ceiling((this.TimelineCanvas.ActualWidth / 2.0d) - closestLine.lineObj.X1) * secondsPerPixel;
                var displayTime = closestLine.lineTime.Value + TimeSpan.FromSeconds(additionSeconds);
                this.DisplayDateTextBlock.Text = $"{displayTime:MM月dd日 (ddd)}";
                this.DisplayTimeTextBlock.Text = $"{displayTime:HH:mm:ss}";

                this._preDrawnTime = displayTime;

                // 録画済み時間帯の描画をおこなう
                var playTimeXPos = (int)Math.Ceiling(TimelineCanvas.ActualWidth / 2.0d);
                var firstTime = displayTime - TimeSpan.FromSeconds(playTimeXPos * secondsPerPixel);
                var lastTime = displayTime + TimeSpan.FromSeconds((TimelineCanvas.ActualWidth - playTimeXPos) * secondsPerPixel);

                foreach (var recordedTermRectangle in _recordedTermRectangles)
                {
                    TimelineCanvas.Children.Remove(recordedTermRectangle);
                }
                _recordedTermRectangles.Clear();

                var recordedTerms = ViewModel.SearchRecordedTerm(firstTime, lastTime);
                foreach (var (startFrameTime, endFrameTime) in recordedTerms)
                {
                    // 描画開始位置を計算する
                    var timeLagForStartFrameTime = displayTime - startFrameTime;
                    var startFrameTimeXPos = playTimeXPos - (int)Math.Ceiling((decimal)timeLagForStartFrameTime.TotalSeconds / (decimal)secondsPerPixel);
                    if (startFrameTimeXPos < 0)
                        startFrameTimeXPos = 0;

                    // 描画終了位置を計算する
                    var timeLagForEndFrameTime = endFrameTime - displayTime;
                    var endFrameTimeXPos = playTimeXPos + (int)Math.Ceiling((decimal)timeLagForEndFrameTime.TotalSeconds / (decimal)secondsPerPixel);
                    if (this.TimelineCanvas.ActualWidth < endFrameTimeXPos)
                        endFrameTimeXPos = (int)this.TimelineCanvas.ActualWidth;

                    var recordedTermRectangle = new Polygon()
                    {
                        Stroke = new SolidColorBrush(Colors.Chartreuse),
                        Fill = new SolidColorBrush(Colors.Chartreuse),
                        FillRule = FillRule.Nonzero,
                        IsHitTestVisible = false
                    };
                    recordedTermRectangle.Points.Add(new Point(startFrameTimeXPos, 20.0d));
                    recordedTermRectangle.Points.Add(new Point(endFrameTimeXPos, 20.0d));
                    recordedTermRectangle.Points.Add(new Point(endFrameTimeXPos, 33.0d));
                    recordedTermRectangle.Points.Add(new Point(startFrameTimeXPos, 33.0d));
                    Canvas.SetZIndex(recordedTermRectangle, 0);
                    TimelineCanvas.Children.Add(recordedTermRectangle);
                    _recordedTermRectangles.AddLast(recordedTermRectangle);
                }
            }
        }

        private static DateTime RoundUp(DateTime input, TimeSpan interval)
            => new DateTime(((input.Ticks + interval.Ticks - 1) / interval.Ticks) * interval.Ticks, input.Kind);

        private static DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero) return dateTime;
            return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
        }
    }
}
