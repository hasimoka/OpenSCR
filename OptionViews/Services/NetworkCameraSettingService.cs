using HalationGhost;
using OpenSCRLib;
using OptionViews.ViewModels;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CameraClient.Models.NetworkCamera;

namespace OptionViews.Services
{
    public class NetworkCameraSettingService : BindableModelBase, INetworkCameraSettingService
    {
        private readonly NetworkCameraClient _cameraClient;

        private readonly WsDiscoveryClient _wsDiscoveryClient;

        private readonly IContainerProvider _container = null;

        public NetworkCameraSettingService(IContainerProvider injectionContainer)
        {
            this._container = injectionContainer;

            this._cameraClient = new NetworkCameraClient();

            this._wsDiscoveryClient = new WsDiscoveryClient();

            this.UserName = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(this.Disposable);

            this.Password = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(this.Disposable);

            this.IsLoggedIn = new ReactivePropertySlim<bool>(false)
                .AddTo(this.Disposable);

            this.FrameImage = this._cameraClient.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.Disposable);

            this.CameraDeviceListSource = new ReactiveCollection<NetworkCameraDeviceListItemViewModel>()
                .AddTo(this.Disposable);

            this.CameraDeviceSelectedItem = new ReactiveProperty<NetworkCameraDeviceListItemViewModel>()
                .AddTo(this.Disposable);

            this.NetworkCameraProfileItems = new ReactiveCollection<OnvifNetworkCameraProfile>()
                .AddTo(this.Disposable);

            this.SelectedNetworkCameraProfile = new ReactiveProperty<OnvifNetworkCameraProfile>()
                .AddTo(this.Disposable);

            this.FrameRateLimit = new ReactiveProperty<int>()
                .AddTo(this.Disposable);

            this.BitRateLimit = new ReactiveProperty<int>()
                .AddTo(this.Disposable);

            this.CanSelectionChangedCameraDeviceListCommand = new ReactivePropertySlim<bool>(true)
                .AddTo(this.Disposable);
        }

        public ReactivePropertySlim<string> UserName { get; }

        public ReactivePropertySlim<string> Password { get; }

        public ReactivePropertySlim<bool> IsLoggedIn { get; }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public ReactiveCollection<NetworkCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<NetworkCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveCollection<OnvifNetworkCameraProfile> NetworkCameraProfileItems { get; }

        public ReactiveProperty<OnvifNetworkCameraProfile> SelectedNetworkCameraProfile { get; }

        public ReactiveProperty<int> FrameRateLimit { get; }

        public ReactiveProperty<int> BitRateLimit { get; }

        public ReactivePropertySlim<bool> CanSelectionChangedCameraDeviceListCommand { get; }

        public void Clear()
        {
            // キャプチャを停止する
            this._cameraClient.StopCapture();

            this.UserName.Value = string.Empty;

            this.Password.Value = string.Empty;

            this.IsLoggedIn.Value = false;

            this._cameraClient.ClearFrameImage();

            this.CameraDeviceListSource.Clear();

            this.CameraDeviceSelectedItem.Value = null;

            this.NetworkCameraProfileItems.Clear();

            this.SelectedNetworkCameraProfile.Value = null;

            this.FrameRateLimit.Value = 0;

            this.BitRateLimit.Value = 0;
        }

        public async Task RefreshCameraList()
        {
            var selectedProfileToken = this.SelectedNetworkCameraProfile.Value;
            this.SelectedNetworkCameraProfile.Value = null;
            this.NetworkCameraProfileItems.Clear();

            var selectedCameraDeviceItem = this.CameraDeviceSelectedItem.Value;
            this.CameraDeviceSelectedItem.Value = null;
            this.CameraDeviceListSource.Clear();

            var foundCameraDevice = false;
            try
            {
                var endpoints = await this.DiscoveryNetworkVideoTransmitterAsync();
                foreach (var endpoint in endpoints.OrderBy(device => device.EndpointUri.Host))
                {
                    var item = this._container.Resolve<NetworkCameraDeviceListItemViewModel>();
                    item.DeviceName.Value = endpoint.DeviceName;
                    item.IpAddress.Value = endpoint.EndpointUri.Host;
                    item.EndpointUri = endpoint.EndpointUri;
                    item.Location = endpoint.Location;

                    this.CameraDeviceListSource.Add(item);

                    if (selectedCameraDeviceItem != null)
                    {
                        if (endpoint.EndpointUri.Host == selectedCameraDeviceItem.IpAddress.Value)
                        {
                            this.CameraDeviceSelectedItem.Value = item;
                            foundCameraDevice = true;
                        }
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
                // 接続するカメラのIPアドレスを取得する
                // ※デッドロック対策のため、別スレッドで実施する
                var profiles = await Task.Run(() =>
                {
                    return this.GetCameraDeviceInfoAsync();
                });

                foreach (var profile in profiles.OrderBy(x => x.ProfileToken))
                {
                    this.NetworkCameraProfileItems.Add(profile);

                    if (foundCameraDevice)
                    {
                        if (selectedProfileToken != null)
                        {
                            if (profile.ProfileToken == selectedProfileToken.ProfileToken)
                            {
                                this.FrameRateLimit.Value = profile.FrameRateLimit;
                                this.BitRateLimit.Value = profile.BitRateLimit;

                                this.SelectedNetworkCameraProfile.Value = profile;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<List<NetworkCameraEndpoint>> DiscoveryNetworkVideoTransmitterAsync()
        {
            return await this._wsDiscoveryClient.DiscoveryNetworkVideoTransmitterAsync();
        }

        public async Task<List<OnvifNetworkCameraProfile>> GetCameraDeviceInfoAsync()
        {
            // カメラにログインするユーザ名／パスワードを取得する
            var name = string.Empty;
            var password = string.Empty;
            if (this.IsLoggedIn.Value)
            {
                name = this.UserName.Value;
                password = this.Password.Value;
            }

            var host = string.Empty;
            if (this.CameraDeviceSelectedItem.Value != null)
            {
                host = this.CameraDeviceSelectedItem.Value.IpAddress.Value;
            }

            return await this._cameraClient.GetCameraDeviceInfoAsync(host, name, password);
        }

        public async Task SetCameraDeviceInfoAsync()
        {
            // カメラにログインするユーザ名／パスワードを取得する
            var name = string.Empty;
            var password = string.Empty;
            if (this.IsLoggedIn.Value)
            {
                name = this.UserName.Value;
                password = this.Password.Value;
            }

            var host = this.CameraDeviceSelectedItem.Value.IpAddress.Value;

            await this._cameraClient.SetCameraDeviceInfoAsync(host, name, password, this.SelectedNetworkCameraProfile.Value.ProfileToken, this.BitRateLimit.Value, 1, this.FrameRateLimit.Value);
        }

        public async Task MoveAsync(PtzDirection ptzDirection)
        {
            // カメラにログインするユーザ名／パスワードを取得する
            var name = string.Empty;
            var password = string.Empty;
            if (this.IsLoggedIn.Value)
            {
                name = this.UserName.Value;
                password = this.Password.Value;
            }

            var host = this.CameraDeviceSelectedItem.Value.IpAddress.Value;
            var profileToken = this.SelectedNetworkCameraProfile.Value.ProfileToken;

            await this._cameraClient.MoveAsync(host, name, password, profileToken, ptzDirection);
        }

        public void StartCapture()
        {
            // カメラにログインするユーザ名／パスワードを取得する
            var userName = string.Empty;
            var password = string.Empty;
            if (this.IsLoggedIn.Value)
            {
                userName = this.UserName.Value;
                password = this.Password.Value;
            }

            var cameraSettings = new CameraSetting
            {
                NetworkCameraSettings = new NetworkCameraSetting
                {
                    StreamUri = this.SelectedNetworkCameraProfile.Value.StreamUri,
                    UserName = userName,
                    Password = password
                }
            };

            this._cameraClient?.StartCapture(cameraSettings);
        }

        public void StopCapture()
        {
            this._cameraClient?.StopCapture();
        }
    }
}
