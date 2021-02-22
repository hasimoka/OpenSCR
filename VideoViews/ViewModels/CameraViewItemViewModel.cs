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
using MainWindowServices;

namespace VideoViews.ViewModels
{
    public class CameraViewItemViewModel : BindableModelBase
    {
        public CameraViewItemViewModel(CaptureCameraClient cameraClient)
        {
            this.CameraName = new ReactivePropertySlim<string>(cameraClient.CameraName)
                .AddTo(this.Disposable);

            this.FrameImage = cameraClient.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.Disposable);

            this.ImageButtonClickCommand = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    await Task.Run(() =>
                    {
                        Console.WriteLine($"Call ImageButtonClickCommand. ({cameraClient.CameraChannel} ch)");
                    });
                })
                .AddTo(this.Disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; private set; }

        public ReactivePropertySlim<string> CameraName { get; private set; }

        public AsyncReactiveCommand ImageButtonClickCommand { get; private set; }
    }
}
