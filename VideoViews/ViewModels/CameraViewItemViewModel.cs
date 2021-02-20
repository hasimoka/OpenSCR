using DirectShowCameraClient.Models;
using HalationGhost;
using HalationGhost.WinApps;
using OnvifNetworkCameraClient.Models;
using OpenSCRLib;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VideoViews.ViewModels
{
    public class CameraViewItemViewModel : BindableModelBase
    {
        private CameraSetting _cameraSetting;

        private readonly NetworkCameraClient _networkCameraClient;

        private readonly UsbCameraClient _usbCameraClient;

        public CameraViewItemViewModel(CameraSetting cameraSetting)
        {
            this._cameraSetting = cameraSetting;

            this.CameraName = new ReactivePropertySlim<string>()
                .AddTo(this.Disposable);

            if (this._cameraSetting.NetworkCameraSetting != null)
            {
                this._networkCameraClient = new NetworkCameraClient(cameraSetting.CameraChannel);
                this._usbCameraClient = null;

                this.FrameImage = this._networkCameraClient.FrameImage
                    .ToReactivePropertySlimAsSynchronized(x => x.Value)
                    .AddTo(this.Disposable);

                // カメラにログインするユーザ名／パスワードを取得する
                var name = string.Empty;
                var password = string.Empty;
                if (this._cameraSetting.NetworkCameraSetting.IsLoggedIn)
                {
                    name = this._cameraSetting.NetworkCameraSetting.UserName;
                    password = this._cameraSetting.NetworkCameraSetting.Password;
                }

                this._networkCameraClient.StartCapture(this._cameraSetting.NetworkCameraSetting.StreamUri, name, password);

            }
            else if (this._cameraSetting.UsbCameraSetting != null)
            {
                this._networkCameraClient = null;
                this._usbCameraClient = new UsbCameraClient(cameraSetting.CameraChannel);

                this.FrameImage = this._usbCameraClient.FrameImage
                    .ToReactivePropertySlimAsSynchronized(x => x.Value)
                    .AddTo(this.Disposable);

                this._usbCameraClient.StartCapture(
                    this._cameraSetting.UsbCameraSetting.DevicePath,
                    this._cameraSetting.UsbCameraSetting.CameraWidth,
                    this._cameraSetting.UsbCameraSetting.CameraHeight,
                    this._cameraSetting.UsbCameraSetting.FrameRate);
            }
            else
            {
                throw new ArgumentException("カメラ種類が設定されていないカメラ設定が引数に指定されました。");
            }

            this.CameraName.Value = this._cameraSetting.CameraName;

            this.ImageButtonClickCommand = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    await Task.Run(() =>
                    {
                        Console.WriteLine($"Call ImageButtonClickCommand. ({this._cameraSetting.CameraChannel} ch)");
                    });
                })
                .AddTo(this.Disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; private set; }

        public ReactivePropertySlim<string> CameraName { get; private set; }

        public AsyncReactiveCommand ImageButtonClickCommand { get; private set; }

        public void StopCapture()
        {
            if (this._networkCameraClient != null)
            {
                this._networkCameraClient.StopCapture();
            }

            if (this._usbCameraClient != null)
            {
                this._usbCameraClient.StopCapture();
            }
        }
    }
}
