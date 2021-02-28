using OpenSCRLib;
using OptionViews.ViewModels;
using Reactive.Bindings;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CameraClient.Models.NetworkCamera;

namespace OptionViews.Services
{
    public interface INetworkCameraSettingService
    {
        ReactivePropertySlim<string> UserName { get; }

        ReactivePropertySlim<string> Password { get; }

        ReactivePropertySlim<bool> IsLoggedIn { get; }

        ReactivePropertySlim<BitmapSource> FrameImage { get; }

        ReactiveCollection<NetworkCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        ReactiveProperty<NetworkCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        ReactiveCollection<OnvifNetworkCameraProfile> NetworkCameraProfileItems { get; }

        ReactiveProperty<OnvifNetworkCameraProfile> SelectedNetworkCameraProfile { get; }

        ReactiveProperty<int> FrameRateLimit { get; }

        ReactiveProperty<int> BitRateLimit { get; }

        ReactivePropertySlim<bool> CanSelectionChangedCameraDeviceListCommand { get; }

        void Clear();

        Task RefreshCameraList();

        Task<List<NetworkCameraEndpoint>> DiscoveryNetworkVideoTransmitterAsync();

        Task<List<OnvifNetworkCameraProfile>> GetCameraDeviceInfoAsync();

        Task SetCameraDeviceInfoAsync();

        Task MoveAsync(PtzDirection ptzDirection);

        void StartCapture();

        void StopCapture();
    }
}
