using HalationGhost.WinApps;
using MainWindowServices;
using OpenSCRLib;
using OptionViews.Services;
using OptionViews.Views;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OptionViews.ViewModels
{
    class NetworkCameraListPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        /// <summary>MainWindoサービスを表します</summary>
        private IMainWindowService mainWindowService;

        private INetworkCameraSettingService networkCameraSettingService;

        public NetworkCameraListPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService, INetworkCameraSettingService cameraSettingService) : base(regionMan)
        {
            this.container = containerProvider;
            this.mainWindowService = windowService;
            this.networkCameraSettingService = cameraSettingService;

            if (this.networkCameraSettingService.IsLoggedIn.Value)
            {
                // ログイン状態に合わせたPanelを設定する
                regionMan.RegisterViewWithRegion("CameraLoginSettingRegion", typeof(CameraLogoutSettingPanel));
            }
            else
            {
                // ログアウト状態に合わせたPanelを設定する
                regionMan.RegisterViewWithRegion("CameraLoginSettingRegion", typeof(CameraLoginSettingPanel));
            }

            this.CameraDeviceListSource = this.networkCameraSettingService.CameraDeviceListSource
                .AddTo(this.disposable);

            this.CameraDeviceSelectedItem = this.networkCameraSettingService.CameraDeviceSelectedItem
                .AddTo(this.disposable);

            this.IsProgressRingDialogOpen = this.mainWindowService.IsProgressRingDialogOpen
                .AddTo(this.disposable);

            this.CanSelectionChangedCameraDeviceListCommand = this.networkCameraSettingService.CanSelectionChangedCameraDeviceListCommand
                .AddTo(this.disposable);

            this.SelectionChangedCameraDeviceListCommand = this.CanSelectionChangedCameraDeviceListCommand
                .ToAsyncReactiveCommand<SelectionChangedEventArgs>()
                .WithSubscribe(async (item) =>
                {
                    // ProgressRingダイアログを表示にする
                    this.IsProgressRingDialogOpen.Value = true;

                    await this.onSelectionChangedCameraDeviceListAsync(item);

                    // ProgressRingダイアログを非表示にする
                    this.IsProgressRingDialogOpen.Value = false;
                })
                .AddTo(this.disposable);
            this.SetInitializeParameterCommand = this.CanSelectionChangedCameraDeviceListCommand
                .ToAsyncReactiveCommand<NetworkCameraSetting>()
                .WithSubscribe(async (networkCameraSetting) =>
                {
                    // ProgressRingダイアログを表示にする
                    this.IsProgressRingDialogOpen.Value = true;

                    await this.onSetInitializeParameterCommand(networkCameraSetting);

                    // ProgressRingダイアログを非表示にする
                    this.IsProgressRingDialogOpen.Value = false;
                })
                .AddTo(this.disposable);
        }

        public ReactiveCollection<NetworkCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<NetworkCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactivePropertySlim<bool> CanSelectionChangedCameraDeviceListCommand { get; }

        public AsyncReactiveCommand<SelectionChangedEventArgs> SelectionChangedCameraDeviceListCommand { get; }

        public AsyncReactiveCommand<NetworkCameraSetting> SetInitializeParameterCommand { get; }

        public ReactivePropertySlim<bool> IsProgressRingDialogOpen { get; set; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var networkCameraSetting = navigationContext.Parameters["NetworkCameraSettings"] as NetworkCameraSetting;
            this.SetInitializeParameterCommand.Execute(networkCameraSetting);
        }

        private async Task onSelectionChangedCameraDeviceListAsync(SelectionChangedEventArgs args)
        {
            if (this.CameraDeviceSelectedItem.Value != null)
            {
                try
                {
                    // 接続するカメラのIPアドレスを取得する
                    // ※デッドロック対策のため、別スレッドで実施する
                    var ipAddress = this.CameraDeviceSelectedItem.Value.IpAddress.Value;
                    var profiles = await Task.Run(() =>
                    {
                        return this.networkCameraSettingService.GetCameraDeviceInfoAsync();
                    });

                    this.networkCameraSettingService.NetworkCameraProfileItems.Clear();

                    foreach (var profile in profiles.OrderBy(x => x.ProfileToken))
                    {
                        this.networkCameraSettingService.NetworkCameraProfileItems.Add(profile);
                    }

                    var firstProfile = this.networkCameraSettingService.NetworkCameraProfileItems.FirstOrDefault();

                    this.networkCameraSettingService.SelectedNetworkCameraProfile.Value = firstProfile;
                    this.networkCameraSettingService.FrameRateLimit.Value = firstProfile.FrameRateLimit;
                    this.networkCameraSettingService.BitRateLimit.Value = firstProfile.BitRateLimit;
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
                this.networkCameraSettingService.BitRateLimit.Value = 0;
            }
        }

        private async Task onSetInitializeParameterCommand(NetworkCameraSetting networkCameraSetting)
        {
            // 現在の設定をすべてクリアする
            this.networkCameraSettingService.Clear();

            if (networkCameraSetting != null)
            {
                this.networkCameraSettingService.UserName.Value = networkCameraSetting.UserName;
                this.networkCameraSettingService.Password.Value = networkCameraSetting.Password;
                this.networkCameraSettingService.IsLoggedIn.Value = networkCameraSetting.IsLoggedIn;
                if (this.networkCameraSettingService.IsLoggedIn.Value)
                {
                    this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLogoutSettingPanel");
                }
                else
                {
                    this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLoginSettingPanel");
                }

                try
                {
                    var selectedIpAddress = networkCameraSetting.IpAddress;

                    var endpoints = await this.networkCameraSettingService.DiscoveryNetworkVideoTransmitterAsync();
                    foreach (var endpoint in endpoints.OrderBy(device => device.EndpointUri.Host))
                    {
                        var item = this.container.Resolve<NetworkCameraDeviceListItemViewModel>();
                        item.DeviceName.Value = endpoint.DeviceName;
                        item.IpAddress.Value = endpoint.EndpointUri.Host;
                        item.EndpointUri = endpoint.EndpointUri;
                        item.Location = endpoint.Location;

                        this.CameraDeviceListSource.Add(item);

                        if (endpoint.EndpointUri.Host == selectedIpAddress)
                        {
                            this.CameraDeviceSelectedItem.Value = item;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                // 制御を一旦手放して、SelectionChangedEventを発火させる
                // ※イベントは発火するが、ReactiveCommandのCanExecuteがfalseになっているのでコマンドは実行されない
                await Task.Delay(TimeSpan.FromMilliseconds(100));

                try
                {
                    var selectedProfileToken = networkCameraSetting.ProfileToken;

                    // 接続するカメラのIPアドレスを取得する
                    // ※デッドロック対策のため、別スレッドで実施する
                    var ipAddress = this.CameraDeviceSelectedItem.Value.IpAddress.Value;
                    var profiles = await Task.Run(() =>
                    {
                        return this.networkCameraSettingService.GetCameraDeviceInfoAsync();
                    });

                    foreach (var profile in profiles.OrderBy(x => x.ProfileToken))
                    {
                        this.networkCameraSettingService.NetworkCameraProfileItems.Add(profile);

                        if (profile.ProfileToken == selectedProfileToken)
                        {
                            this.networkCameraSettingService.FrameRateLimit.Value = profile.FrameRateLimit;
                            this.networkCameraSettingService.BitRateLimit.Value = profile.BitRateLimit;

                            this.networkCameraSettingService.SelectedNetworkCameraProfile.Value = profile;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLoginSettingPanel");

                try
                {
                    var endpoints = await this.networkCameraSettingService.DiscoveryNetworkVideoTransmitterAsync();
                    foreach (var endpoint in endpoints.OrderBy(device => device.EndpointUri.Host))
                    {
                        var item = this.container.Resolve<NetworkCameraDeviceListItemViewModel>();
                        item.DeviceName.Value = endpoint.DeviceName;
                        item.IpAddress.Value = endpoint.EndpointUri.Host;
                        item.EndpointUri = endpoint.EndpointUri;
                        item.Location = endpoint.Location;

                        this.CameraDeviceListSource.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
