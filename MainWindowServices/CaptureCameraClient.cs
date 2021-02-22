using DirectShowCameraClient.Models;
using OnvifNetworkCameraClient.Models;
using OpenSCRLib;
using System;
using System.Windows.Media.Imaging;
using HalationGhost;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace MainWindowServices
{
    public class CaptureCameraClient: BindableModelBase, IDisposable
    {
        private readonly CameraSetting _cameraSettings;

        private readonly ICameraClient _cameraClient;

        public CaptureCameraClient(CameraSetting cameraSettings)
        {
            _cameraSettings = cameraSettings;

            if (_cameraSettings.NetworkCameraSettings != null)
            {
                _cameraClient = new NetworkCameraClient(cameraSettings.CameraChannel);
            }
            else if (_cameraSettings.UsbCameraSettings != null)
            {
                _cameraClient = new UsbCameraClient(cameraSettings.CameraChannel);
            }
            else
            {
                throw new ArgumentException("カメラ種類が設定されていないカメラ設定が引数に指定されました。");
            }

            FrameImage = _cameraClient.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.Disposable);

            this._cameraClient.StartCapture(_cameraSettings);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public int CameraChannel => _cameraSettings?.CameraChannel ?? -1;

        public string CameraName => _cameraSettings?.CameraName ?? string.Empty;

        public new void Dispose()
        {
            _cameraSettings?.Dispose();
            _cameraClient?.Dispose();

            base.Dispose();
        }

        public void StartCapture()
        {
            _cameraClient?.StartCapture(_cameraSettings);
        }

        public void StopCapture()
        {
            _cameraClient?.StopCapture();
        }

        public void ClearFrameImage()
        {
            _cameraClient?.ClearFrameImage();
        }
    }
}
