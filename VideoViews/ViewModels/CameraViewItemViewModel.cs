using CameraClient.Models;
using HalationGhost;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Windows.Media.Imaging;

namespace VideoViews.ViewModels
{
    public class CameraViewItemViewModel : BindableModelBase
    {
        private readonly IRegionManager _regionManager;

        public CameraViewItemViewModel(CaptureCameraClient cameraClient, IRegionManager regionMan)
        {
            _regionManager = regionMan;

            CameraName = new ReactivePropertySlim<string>(cameraClient.CameraName)
                .AddTo(Disposable);

            FrameImage = cameraClient.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposable);

            ImageButtonClickCommand = new ReactiveCommand()
                .WithSubscribe(() =>
                {
                    var streamingPanelParameter = new NavigationParameters
                    {
                        {"CaptureCameraClient", cameraClient}
                    };
                    _regionManager.RequestNavigate("ContentRegion", "StreamingPanel", streamingPanelParameter);
                })
                .AddTo(Disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; private set; }

        public ReactivePropertySlim<string> CameraName { get; private set; }

        public ReactiveCommand ImageButtonClickCommand { get; private set; }
    }
}
