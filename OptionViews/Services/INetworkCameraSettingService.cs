using OpenSCRLib;
using OptionViews.AdvancedCameraSettings;
using OptionViews.Models;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OptionViews.Services
{
    public interface INetworkCameraSettingService
    {
        ReactivePropertySlim<BitmapSource> FrameImage { get; }

        ReactiveCollection<NetworkCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        ReactiveProperty<NetworkCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        ReactiveCollection<OnvifNetworkCameraProfile> NetworkCameraProfileItems { get; }

        ReactiveProperty<OnvifNetworkCameraProfile> SelectedNetworkCameraProfile { get; }

        ReactiveProperty<int> FrameRateLimit { get; }

        ReactiveProperty<int> BitrateLimit { get; }

        void FindNetworkVideoTransmitterAsync(Action<IpCameraDeviceInfo> deviceDiscoveriedAction);

        Task<List<OnvifNetworkCameraProfile>> GetCameraDeviceInfoAsync(string host, string userName, string password);

        void StartCapture();
    }
}
