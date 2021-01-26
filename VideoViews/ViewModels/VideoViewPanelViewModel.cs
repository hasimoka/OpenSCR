using HalationGhost.WinApps;
using MainWindowServices;
using OpenSCRLib;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoViews.ViewModels
{
    class VideoViewPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        private IMainWindowService mainWindowService;

        public VideoViewPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService) : base(regionMan)
        {
            this.container = containerProvider;
            this.mainWindowService = windowService;

            this.ConnectedDeviceCount = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(this.disposable);

            this.CameraViewItems = new ReactiveCollection<CameraViewItemViewModel>()
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<string> ConnectedDeviceCount { get; }

        public ReactiveCollection<CameraViewItemViewModel> CameraViewItems { get; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext) 
        {
            this.CameraViewItems.Clear();

            // DBからカメラ設定を取得する
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            var loadedSettings = dbAccessor.FindCameraSettings();

            var settingCount = loadedSettings.Count;

            var connectedCount = 0;
            foreach (var loadedSetting in loadedSettings)
            {
                var setting = this.container.Resolve<CameraViewItemViewModel>((typeof(CameraSetting), loadedSetting));
                this.CameraViewItems.Add(setting);
                connectedCount += 1;
            }

            this.SetConnectedDeivceCount(settingCount, connectedCount);
        }

        private void SetConnectedDeivceCount(int settingCount, int connectedCount)
        {
            this.ConnectedDeviceCount.Value = $"{connectedCount} / {settingCount}";
        }
    }
}
