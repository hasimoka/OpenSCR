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
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public ReactiveCollection<NetworkCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<NetworkCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveCollection<OnvifNetworkCameraProfile> NetworkCameraProfileItems { get; }

        public ReactiveProperty<OnvifNetworkCameraProfile> SelectedNetworkCameraProfile { get; }

        public ReactiveProperty<int> FrameRateLimit { get; }

        public ReactiveProperty<int> BitrateLimit { get; }

        public void FindNetworkVideoTransmitterAsync(Action<IpCameraDeviceInfo> deviceDiscoveriedAction)
        {
            this.wsDiscoveryClient.FindNetworkVideoTransmitterAsync(deviceDiscoveriedAction);
        }

        public async Task<List<OnvifNetworkCameraProfile>> GetCameraDeviceInfoAsync(string host, string userName, string password)
        {
            return await this.cameraClient.GetCameraDeviceInfoAsync(host, userName, password);
        }

        public void StartCapture()
        {
            // カメラにログインするユーザ名／パスワードを取得する
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();

            var cameraLoginInfo = dbAccessor.GetCameraLoginInfo();
            var name = string.Empty;
            var password = string.Empty;

            if (cameraLoginInfo != null)
            {
                name = cameraLoginInfo.Name;
                password = cameraLoginInfo.Password;
            }

            this.cameraClient.StartCapture(this.SelectedNetworkCameraProfile.Value.StreamUri, name, password);
        }

        public void StopCapture()
        {
            this.cameraClient.StopCapture();
        }
    }
}
