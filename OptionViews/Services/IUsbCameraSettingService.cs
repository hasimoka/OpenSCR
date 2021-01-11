using DirectShowUsbCameraClient;
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

        ReactiveProperty<UsbCameraDeviceInfo> SelectedUsbCameraDeviceInfo { get; set; }

        ReactiveCollection<UsbCameraVideoInfo> UsbCameraVideoInfoItems { get; set; }

        ReactiveProperty<UsbCameraVideoInfo> SelectedUsbCameraVideoInfo { get; set; }

        void FindCaptureDeviceAsync(Action<UsbCameraDeviceInfo> discoveriedAction);

        List<UsbCameraVideoInfo> GetVideoInfosAsync(UsbCameraDeviceInfo captureDevice);

        void StartCapture();
    }
}
