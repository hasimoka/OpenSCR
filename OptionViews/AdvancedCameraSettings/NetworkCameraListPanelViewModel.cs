using HalationGhost.WinApps;
using MainWindowServices;
using OpenSCRLib;
using OptionViews.CameraLoginSettings;
using OptionViews.CameraLogoutSettings;
using OptionViews.Models;
using OptionViews.Services;
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
    class NetworkCameraListPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        /// <summary>MainWindoサービスを表します</summary>
        private IMainWindowService mainWindowService;

        private INetworkCameraSettingService networkCameraSettingService;

        private Action<IpCameraDeviceInfo> cameraDiscoveryedAction;

        public NetworkCameraListPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService, INetworkCameraSettingService cameraSettingService) : base(regionMan)
        {
            this.container = containerProvider;
            this.mainWindowService = windowService;
            this.networkCameraSettingService = cameraSettingService;

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

            this.CameraDeviceListSource = this.networkCameraSettingService.CameraDeviceListSource
                .AddTo(this.disposable);

            this.CameraDeviceSelectedItem = this.networkCameraSettingService.CameraDeviceSelectedItem
                .AddTo(this.disposable);

            this.cameraDiscoveryedAction = (cameraDeviceInfo) =>
            {
                var item = this.container.Resolve<NetworkCameraDeviceListItemViewModel>();
                item.DeviceName.Value = cameraDeviceInfo.Name;
                item.IpAddress.Value = cameraDeviceInfo.Address;
                item.EndpointUri = cameraDeviceInfo.EndpointUri;
                item.Location = cameraDeviceInfo.Location;

                this.CameraDeviceListSource.Add(item);
            };
            this.networkCameraSettingService.FindNetworkVideoTransmitterAsync(this.cameraDiscoveryedAction);

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

        public ReactiveCollection<NetworkCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<NetworkCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

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
            this.networkCameraSettingService.FindNetworkVideoTransmitterAsync(this.cameraDiscoveryedAction);
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
                        return this.networkCameraSettingService.GetCameraDeviceInfoAsync(ipAddress, userName, password);
                    });

                    this.networkCameraSettingService.NetworkCameraProfileItems.Clear();

                    foreach (var profile in profiles.OrderBy(x => x.ProfileToken))
                    {
                        this.networkCameraSettingService.NetworkCameraProfileItems.Add(profile);
                    }

                    var firstProfile = this.networkCameraSettingService.NetworkCameraProfileItems.FirstOrDefault();

                    this.networkCameraSettingService.SelectedNetworkCameraProfile.Value = firstProfile;
                    this.networkCameraSettingService.FrameRateLimit.Value = firstProfile.FrameRateLimit;
                    this.networkCameraSettingService.BitrateLimit.Value = firstProfile.BitrateLimite;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                this.networkCameraSettingService.NetworkCameraProfileItems.Clear();

                this.networkCameraSettingService.SelectedNetworkCameraProfile.Value = null;
                this.networkCameraSettingService.FrameRateLimit.Value = 0;
                this.networkCameraSettingService.BitrateLimit.Value = 0;
            }

            // ProgressRingダイアログを非表示にする
            this.IsProgressRingDialogOpen.Value = false;
        }
    }
}
