using HalationGhost.WinApps;
using MainWindowServices;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CameraClient.Models;
using VideoViews.Views;

namespace VideoViews.ViewModels
{
    public class StreamingPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private readonly IContainerProvider _container;

        private readonly IMainWindowService _mainWindowService;

        private readonly DispatcherTimer _dispatcherTimer;

        private CaptureCameraClient _cameraClient;

        private TimeSpan _timeSpanBetweenPlaytimeAndCurrent;

        public StreamingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService) : base(regionMan)
        {
            _container = containerProvider;
            _mainWindowService = windowService;

            // --
            // 録画表示時に使用するタイマイベント
            // --

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            // 150ms間隔に設定
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            // NoImageの画像を読み込む
            FrameImage = new ReactivePropertySlim<BitmapSource>(new BitmapImage(new Uri(Path.GetFullPath(@".\Images\no_image_g.png"))))
                .AddTo(disposable);
            //FrameImage = null;
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; private set; }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["CaptureCameraClient"] is CaptureCameraClient cameraClient)
            {
                Console.WriteLine($"CameraChannel={cameraClient.CameraChannel}");
                _cameraClient = cameraClient;

                FrameImage?.Dispose();
                FrameImage = _cameraClient.FrameImage
                    .ToReactivePropertySlimAsSynchronized(x => x.Value)
                    .AddTo(disposable);
                RaisePropertyChanged(nameof(FrameImage));

                _dispatcherTimer.Start();
            }
            else
            {
                throw new ArgumentException("CaptureCameraClientが設定されていません。");
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            _dispatcherTimer.Stop();
            _cameraClient.Stop();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // 再生表示日時を算出する
            var playingTime = DateTime.Now - this._timeSpanBetweenPlaytimeAndCurrent;

            // Viewへ再生基準時刻を通知する
            Messenger.Instance.GetEvent<PubSubEvent<DateTime>>().Publish(playingTime);
        }

        public void MoveDisplayTime(DateTime displayTime)
        {
            // 現在時刻と基準時刻の差分に移動させる時間量を加算する
            _timeSpanBetweenPlaytimeAndCurrent = DateTime.Now - displayTime;
            if (_timeSpanBetweenPlaytimeAndCurrent < TimeSpan.Zero)
                _timeSpanBetweenPlaytimeAndCurrent = TimeSpan.Zero;

            if (_timeSpanBetweenPlaytimeAndCurrent < TimeSpan.FromSeconds(1))
            {
                _cameraClient.Stop();

                FrameImage?.Dispose();
                FrameImage = _cameraClient.FrameImage
                    .ToReactivePropertySlimAsSynchronized(x => x.Value)
                    .AddTo(disposable);
                RaisePropertyChanged(nameof(FrameImage));
            }
            else
            {
                // 新しい表示時刻から再生する
                _cameraClient.Stop();
                _cameraClient.Play(DateTime.Now - _timeSpanBetweenPlaytimeAndCurrent);

                FrameImage?.Dispose();
                FrameImage = _cameraClient.PlayerFrameImage
                    .ToReactivePropertySlimAsSynchronized(x => x.Value)
                    .AddTo(disposable);
                RaisePropertyChanged(nameof(FrameImage));
            }
        }

        public LinkedList<(DateTime startFrameTime, DateTime endFrameTime)> SearchRecordedTerm(DateTime searchStartTime, DateTime searchEndTime)
        {
            return _cameraClient.SearchRecordedTerms(searchStartTime, searchEndTime);
        }
    }
}
