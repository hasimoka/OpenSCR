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
        private readonly IContainerProvider _container;

        private readonly IMainWindowService _mainWindowService;

        public VideoViewPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService) : base(regionMan)
        {
            this._container = containerProvider;
            this._mainWindowService = windowService;

            this.ConnectedDeviceCount = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(this.disposable);

            this.CameraViewItems = new ReactiveCollection<CameraViewItemViewModel>()
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<string> ConnectedDeviceCount { get; }

        public ReactiveCollection<CameraViewItemViewModel> CameraViewItems { get; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) 
        {
            ClearCameraViewItems();
        }

        public void OnNavigatedTo(NavigationContext navigationContext) 
        {
            ClearCameraViewItems();

            // DBからカメラ設定を取得する
            var settingCount = _mainWindowService.CaptureCameraClients.Count;
            var connectedCount = 0;
            foreach (var cameraClientPair in _mainWindowService.CaptureCameraClients)
            {
                var setting = new CameraViewItemViewModel(cameraClientPair.Value, regionManager);
                this.CameraViewItems.Add(setting);
                connectedCount += 1;
            }

            this.SetConnectedDeviceCount(settingCount, connectedCount);
        }

        private void SetConnectedDeviceCount(int settingCount, int connectedCount)
        {
            this.ConnectedDeviceCount.Value = $"{connectedCount} / {settingCount}";
        }

        private void ClearCameraViewItems()
        {
            foreach (var cameraViewItem in CameraViewItems)
            {
                cameraViewItem.Dispose();
            }

            CameraViewItems.Clear();
        }
    }
}
