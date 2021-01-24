using HalationGhost;
using OnvifNetworkCameraClient.Models;
using OpenSCRLib;
using OptionViews.AdvancedCameraSettings;
using OptionViews.Models;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OptionViews.Services
{
    public class NetworkCameraSettingService : BindableModelBase, INetworkCameraSettingService
    {
        private NetworkCameraClient cameraClient;

        private WsDiscoveryClient wsDiscoveryClient;

        private IContainerProvider container = null;

        public NetworkCameraSettingService(IContainerProvider injectionContainer)
        {
            this.container = injectionContainer;

            this.cameraClient = new NetworkCameraClient();

            this.wsDiscoveryClient = new WsDiscoveryClient();

            this.UserName = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(this.Disposable);

            this.Password = new ReactivePropertySlim<string>(string.Empty)
                .AddTo(this.Disposable);

            this.IsLoggedIn = new ReactivePropertySlim<bool>(false)
                .AddTo(this.Disposable);

            this.FrameImage = this.cameraClient.FrameImage
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

            this.BitrateLimit = new ReactiveProperty<int>()
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

        public ReactiveProperty<int> BitrateLimit { get; }

        public ReactivePropertySlim<bool> CanSelectionChangedCameraDeviceListCommand { get; }

        public void Clear()
        {
            // キャプチャを停止する
            this.cameraClient.StopCapture();

            this.UserName.Value = string.Empty;

            this.Password.Value = string.Empty;

            this.IsLoggedIn.Value = false;

            this.cameraClient.ClearFrameImage();

            this.CameraDeviceListSource.Clear();

            this.CameraDeviceSelectedItem.Value = null;

            this.NetworkCameraProfileItems.Clear();

            this.SelectedNetworkCameraProfile.Value = null;

            this.FrameRateLimit.Value = 0;

            this.BitrateLimit.Value = 0;
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
                    var item = this.container.Resolve<NetworkCameraDeviceListItemViewModel>();
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
                                this.BitrateLimit.Value = profile.BitrateLimite;

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
            return await this.wsDiscoveryClient.DiscoveryNetworkVideoTransmitterAsync();
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

            return await this.cameraClient.GetCameraDeviceInfoAsync(host, name, password);
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

            await this.cameraClient.SetCameraDeviceInfoAsync(host, name, password, this.SelectedNetworkCameraProfile.Value.ProfileToken, this.BitrateLimit.Value, 1, this.FrameRateLimit.Value);
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

            await this.cameraClient.MoveAsync(host, name, password, profileToken, ptzDirection);
        }

        public void StartCapture()
        {
            // カメラにログインするユーザ名／パスワードを取得する
            var name = string.Empty;
            var password = string.Empty;
            if (this.IsLoggedIn.Value)
            {
                name = this.UserName.Value;
                password = this.Password.Value;
            }

            this.cameraClient.StartCapture(this.SelectedNetworkCameraProfile.Value.StreamUri, name, password);
        }

        public void StopCapture()
        {
            this.cameraClient.StopCapture();
        }
    }
}
