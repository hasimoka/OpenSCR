using DirectShowLib;
using OpenSCRLib;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace DirectShowCameraClient.Models
{
    public interface IUsbCameraClient : IDisposable
    {
        ReactivePropertySlim<BitmapSource> FrameImage { get; set; }

        List<UsbCameraVideoInfo> GetVideoInfosAsync(UsbCameraDeviceInfo captureDevice);

        void StartCapture(string devicePath, int width, int height, short bitCount);

        void StopCapture();
    }
}
