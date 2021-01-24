using OpenSCRLib;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DirectShowCameraClient.Models
{
    public interface IUsbCameraClient
    {
        ReactivePropertySlim<BitmapSource> FrameImage { get; set; }

        void FindUsbCaptureDeviceAsync(Action<UsbCameraDeviceInfo> discoveriedAction);

        Task<List<UsbCameraDeviceInfo>> DiscoveryCaptureDeviceAsync();

        Task<List<UsbCameraVideoInfo>> GetVideoInfosAsync(UsbCameraDeviceInfo captureDevice);

        void StartCapture(string devicePath, int width, int height, short bitCount);

        void StopCapture();
        
        void ClearFrameImage();
    }
}
