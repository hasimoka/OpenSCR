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
    public class UsbCameraClient : BindableModelBase, ICameraClient
    {
        private readonly DirectShowCaputer _capture;

        private Action<UsbCameraDeviceInfo> _deviceDiscoveredAction;

        private bool _isRunning;

        private int? _cameraChannel;

        public UsbCameraClient()
        {
            this._cameraChannel = null;

            this._capture = new DirectShowCaputer()
                .AddTo(this.Disposable);

            this.FrameImage = this._capture.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.Disposable);

            this._isRunning = false;
        }

        public UsbCameraClient(int cameraChannel)
        {
            this._cameraChannel = cameraChannel;

            this._capture = new DirectShowCaputer()
                .AddTo(this.Disposable);

            this.FrameImage = this._capture.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.Disposable);

            this._isRunning = false;
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public void StartCapture(CameraSetting cameraSettings)
        {
            if (cameraSettings?.UsbCameraSettings == null)
                return;
            
            if (this._isRunning)
                this.StopCapture();

            this._capture.Start(
                cameraSettings.UsbCameraSettings.DevicePath, 
                cameraSettings.UsbCameraSettings.CameraWidth, 
                cameraSettings.UsbCameraSettings.CameraHeight, 
                cameraSettings.UsbCameraSettings.FrameRate);
            this._isRunning = true;
        }

        public void StopCapture()
        {
            this._capture.Stop();
            this._isRunning = false;
        }

        public void ClearFrameImage()
        {
            this._capture.ClearFrameImage();
        }

        public void FindUsbCaptureDeviceAsync(Action<UsbCameraDeviceInfo> discoveredAction)
        {
            this._deviceDiscoveredAction = discoveredAction;

            var task = Task.Run(() =>
            {
                var devices = DirectShowCaputer.GetCaptureDevices();

                foreach (var device in devices)
                {
                    this._deviceDiscoveredAction(new UsbCameraDeviceInfo(device.DevicePath, device.Name));
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
            return await Task.Run(() => DirectShowCaputer.GetVideoInfos(captureDevice));
        }
    }
}
