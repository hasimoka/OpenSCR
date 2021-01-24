using DirectShowLib;
using HalationGhost;
using OpenSCRLib;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace DirectShowCameraClient.Models
{
    public class UsbCameraClient : BindableModelBase, IUsbCameraClient
    {
        private DirectShowCaputer capture;

        private Action<UsbCameraDeviceInfo> deviceDiscoveriedAction;

        private bool isRunning;

        public UsbCameraClient()
        {
            this.capture = new DirectShowCaputer()
                .AddTo(this.Disposable);

            this.FrameImage = this.capture.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.Disposable);

            this.isRunning = false;
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; set; }

        public void FindUsbCaptureDeviceAsync(Action<UsbCameraDeviceInfo> discoveriedAction)
        {
            this.deviceDiscoveriedAction = discoveriedAction;

            var task = Task.Run(() =>
            {
                var devices = DirectShowCaputer.GetCaptureDevices();

                foreach (var device in devices)
                {
                    this.deviceDiscoveriedAction(new UsbCameraDeviceInfo(device.DevicePath, device.Name));
                }
            });
        }

        public async Task<List<UsbCameraDeviceInfo>> DiscoveryCaptureDeviceAsync()
        {
            return await Task.Run(() =>
            {
                List<UsbCameraDeviceInfo> results = new List<UsbCameraDeviceInfo>();

                var devices = DirectShowCaputer.GetCaptureDevices();

                foreach (var device in devices)
                {
                    results.Add(new UsbCameraDeviceInfo(device.DevicePath, device.Name));
                }

                return results;
            });
        }

        public async Task<List<UsbCameraVideoInfo>> GetVideoInfosAsync(UsbCameraDeviceInfo captureDevice)
        {
            return await Task.Run(() => { return DirectShowCaputer.GetVideoInfos(captureDevice); });
        }

        public void StartCapture(string devicePath, int width, int height, short bitCount)
        {
            if (this.isRunning)
                this.StopCapture();

            this.capture.Start(devicePath, width, height, bitCount);
            this.isRunning = true;
        }

        public void StopCapture()
        {
            this.capture.Stop();
            this.isRunning = false;
        }

        public void ClearFrameImage()
        {
            this.capture.ClearFrameImage();
        }
    }
}
