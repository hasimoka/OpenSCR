using HalationGhost.WinApps;
using MainWindowServices;
using OpenSCRLib;
using OptionViews.CameraLoginSettings;
using OptionViews.CameraLogoutSettings;
using OptionViews.Models;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OptionViews.AdvancedCameraSettings
{
    class IpCameraListPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        private WsDiscoveryClient wsDiscoveryClient;

        /// <summary>MainWindoサービスを表します</summary>
        private IMainWindowService mainWindowService;

        private Action<IpCameraDeviceInfo> cameraDiscoveryedAction;

        public IpCameraListPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService) : base(regionMan)
        {
            this.container = containerProvider;
            this.mainWindowService = windowService;

            // ログイン状態に合わせたPanelを設定する
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            var camearLoginInfo = dbAccessor.GetCameraLoginInfo();

            if (camearLoginInfo.IsLoggedIn)
            {
                regionMan.RegisterViewWithRegion("CameraLoginSettingRegion", typeof(CameraLogoutSettingPanel));
            }
            else
            {
                regionMan.RegisterViewWithRegion("CameraLoginSettingRegion", typeof(CameraLoginSettingPanel));
            }

            this.CameraDeviceListSource = new ReactiveCollection<IpCameraDeviceListItemViewModel>()
                .AddTo(this.disposable);

            this.CameraDeviceSelectedItem = new ReactiveProperty<IpCameraDeviceListItemViewModel>()
                .AddTo(this.disposable);

            this.cameraDiscoveryedAction = (cameraDeviceInfo) =>
            {
                var item = this.container.Resolve<IpCameraDeviceListItemViewModel>();
                item.DeviceName.Value = cameraDeviceInfo.Name;
                item.IpAddress.Value = cameraDeviceInfo.Address;
                item.EndpointUri = cameraDeviceInfo.EndpointUri;
                item.Location = cameraDeviceInfo.Location;

                this.CameraDeviceListSource.Add(item);
            };
            this.wsDiscoveryClient = containerProvider.Resolve<WsDiscoveryClient>();
            this.wsDiscoveryClient.FindNetworkVideoTransmitterAsync(this.cameraDiscoveryedAction);

            this.SelectionChangedCameraDeviceList = new ReactiveCommand<SelectionChangedEventArgs>()
                .WithSubscribe((item) =>
                    {
                        // ProgressRingダイアログを表示にする(onSelectionChangedCameraDeviceListAsync()終了時に非表示にする)
                        this.IsProgressRingDialogOpen.Value = true;
                        this.onSelectionChangedCameraDeviceListAsync(item);
                    })
                .AddTo(this.disposable);

            this.IsProgressRingDialogOpen = this.mainWindowService.IsProgressRingDialogOpen
                .AddTo(this.disposable);
        }

        public ReactiveCollection<IpCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<IpCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveCommand<SelectionChangedEventArgs> SelectionChangedCameraDeviceList { get; }

        public ReactivePropertySlim<bool> IsProgressRingDialogOpen { get; set; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //this.RefreshCameraList();
        }

        public void RefreshCameraList()
        {
            this.CameraDeviceListSource.Clear();
            this.wsDiscoveryClient.FindNetworkVideoTransmitterAsync(this.cameraDiscoveryedAction);
        }

        private async void onSelectionChangedCameraDeviceListAsync(SelectionChangedEventArgs args)
        {
            if (this.CameraDeviceSelectedItem.Value != null)
            {
                // カメラ接続に使用するログイン名／パスワードを取得する
                var dbAccessor = this.container.Resolve<DatabaseAccesser>();

                var cameraLoginInfo = dbAccessor.GetCameraLoginInfo();
                var userName = string.Empty;
                var password = string.Empty;

                if (cameraLoginInfo != null)
                {
                    userName = cameraLoginInfo.Name;
                    password = cameraLoginInfo.Password;
                }

                try
                {
                    // 接続するカメラのIPアドレスを取得する
                    // ※デッドロック対策のため、別スレッドで実施する
                    var ipAddress = this.CameraDeviceSelectedItem.Value.IpAddress.Value;
                    var profiles = await Task.Run(() =>
                    {
                        return OnvifInfoClient.GetCameraDeviceInfoAsync(ipAddress, userName, password);
                    });

                    var view = this.regionManager.Regions["ContentRegion"].ActiveViews.FirstOrDefault() as AdvancedCameraSettingPanel;
                    var viewModel = view.DataContext as AdvancedCameraSettingPanelViewModel;
                    viewModel.SetIpCameraSetting(profiles);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                var view = this.regionManager.Regions["ContentRegion"].ActiveViews.FirstOrDefault() as AdvancedCameraSettingPanel;
                var viewModel = view.DataContext as AdvancedCameraSettingPanelViewModel;
                viewModel.SetIpCameraSetting(null);
            }

            // ProgressRingダイアログを非表示にする
            this.IsProgressRingDialogOpen.Value = false;
        }
    }
}
