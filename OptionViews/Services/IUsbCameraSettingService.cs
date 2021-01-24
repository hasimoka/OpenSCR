using DirectShowCameraClient;
using OpenSCRLib;
using OptionViews.AdvancedCameraSettings;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OptionViews.Services
{
    public interface IUsbCameraSettingService
    {
        ReactivePropertySlim<BitmapSource> FrameImage { get; set; }

        ReactiveCollection<UsbCameraDeviceListItemViewModel> CameraDeviceListSource { get; set; }

        ReactiveProperty<UsbCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        ReactiveCollection<UsbCameraVideoInfo> UsbCameraVideoInfoItems { get; set; }

        ReactiveProperty<UsbCameraVideoInfo> SelectedUsbCameraVideoInfo { get; set; }

        ReactivePropertySlim<bool> CanSelectionChangedCameraDeviceListCommand { get; }

        void Clear();

        Task RefreshCameraList();

        void FindCaptureDeviceAsync(Action<UsbCameraDeviceInfo> discoveriedAction);

        Task<List<UsbCameraDeviceInfo>> DiscoveryCaptureDeviceAsync();

        Task<List<UsbCameraVideoInfo>> GetVideoInfosAsync(UsbCameraDeviceInfo captureDevice);

        void StartCapture();

        void StopCapture();
    }
}
