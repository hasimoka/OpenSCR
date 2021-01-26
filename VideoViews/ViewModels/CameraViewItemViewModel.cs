using DirectShowCameraClient.Models;
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
    public class CameraViewItemViewModel : HalationGhostViewModelBase
    {
        private CameraSetting cameraSetting;

        private IContainerProvider container;

        private NetworkCameraClient networkCameraClient;

        private UsbCameraClient usbCameraClient;

        public CameraViewItemViewModel(CameraSetting cameraSetting, IRegionManager regionMan, IContainerProvider container) : base(regionMan)
        {
            this.cameraSetting = cameraSetting;
            this.container = container;

            this.CameraName = new ReactivePropertySlim<string>()
                .AddTo(this.disposable);

            if (cameraSetting.NetowrkCameraSetting != null)
            {
                this.networkCameraClient = new NetworkCameraClient();
                this.usbCameraClient = null;

                this.FrameImage = this.networkCameraClient.FrameImage
                    .ToReactivePropertySlimAsSynchronized(x => x.Value)
                    .AddTo(this.disposable);

                // カメラにログインするユーザ名／パスワードを取得する
                var name = string.Empty;
                var password = string.Empty;
                if (this.cameraSetting.NetowrkCameraSetting.IsLoggedIn)
                {
                    name = this.cameraSetting.NetowrkCameraSetting.UserName;
                    password = this.cameraSetting.NetowrkCameraSetting.Password;
                }

                this.networkCameraClient.StartCapture(this.cameraSetting.NetowrkCameraSetting.StreamUri, name, password);

            }
            else if (cameraSetting.UsbCameraSetting != null)
            {
                this.networkCameraClient = null;
                this.usbCameraClient = new UsbCameraClient();

                this.FrameImage = this.usbCameraClient.FrameImage
                    .ToReactivePropertySlimAsSynchronized(x => x.Value)
                    .AddTo(this.disposable);

                this.usbCameraClient.StartCapture(
                    this.cameraSetting.UsbCameraSetting.DevicePath,
                    this.cameraSetting.UsbCameraSetting.CameraWidth,
                    this.cameraSetting.UsbCameraSetting.CameraHeight,
                    this.cameraSetting.UsbCameraSetting.FrameRate);
            }
            else
            {
                throw new ArgumentException("カメラ種類が設定されていないカメラ設定が引数に指定されました。");
            }

            this.CameraName.Value = this.cameraSetting.CameraName;


            this.ImageButtonClickCommand = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    await Task.Run(() =>
                    {
                        Console.WriteLine($"Call ImageButtonClickCommand. ({this.cameraSetting.CameraChannel} ch)");
                    });
                })
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public ReactivePropertySlim<string> CameraName { get; }

        public AsyncReactiveCommand ImageButtonClickCommand { get; }
    }
}
