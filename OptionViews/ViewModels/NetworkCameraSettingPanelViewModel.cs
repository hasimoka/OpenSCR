using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CameraClient.Models.NetworkCamera;
using HalationGhost.WinApps;
using OpenSCRLib;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace OptionViews.ViewModels
{
    class NetworkCameraSettingPanelViewModel : HalationGhostViewModelBase
    {
        private IContainerProvider _container;

        private readonly INetworkCameraSettingService _networkCameraSettingService;

        public NetworkCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, INetworkCameraSettingService cameraSettingService) : base(regionMan)
        {
            _container = containerProvider;
            _networkCameraSettingService = cameraSettingService;

            SharedCanExecuteState = new ReactivePropertySlim<bool>(true)
                .AddTo(disposable);

            FrameImage = _networkCameraSettingService.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(disposable);

            NetworkCameraProfileItems = _networkCameraSettingService.NetworkCameraProfileItems
                .AddTo(disposable);

            SelectedNetworkCameraProfile = _networkCameraSettingService.SelectedNetworkCameraProfile
                .SetValidateAttribute(()=>SelectedNetworkCameraProfile)
                .AddTo(disposable);

            NetworkCameraProfileItemSelectionChanged = new ReactiveCommand()
                .WithSubscribe(() => OnNetworkCameraProfileItemSelectionChanged())
                .AddTo(disposable);

            FrameRateLimit = _networkCameraSettingService.FrameRateLimit
                .AddTo(disposable);

            BitRateLimit = _networkCameraSettingService.BitRateLimit
                .AddTo(disposable);

            UpMoveCommand = SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(this.SharedCanExecuteState)
                .WithSubscribe(OnUpMoveCommand)
                .AddTo(this.disposable);

            DownMoveCommand = SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(SharedCanExecuteState)
                .WithSubscribe(OnDownMoveCommand)
                .AddTo(disposable);

            LeftMoveCommand = SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(SharedCanExecuteState)
                .WithSubscribe(OnLeftMoveCommand)
                .AddTo(disposable);

            RightMoveCommand = SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(SharedCanExecuteState)
                .WithSubscribe(OnRightMoveCommand)
                .AddTo(disposable);

            ZoomInCommand = SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(SharedCanExecuteState)
                .WithSubscribe(OnZoomInCommand)
                .AddTo(disposable);

            ZoomOutCommand = SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(SharedCanExecuteState)
                .WithSubscribe(OnZoomOutCommand)
                .AddTo(disposable);
        }

        public ReactivePropertySlim<bool> SharedCanExecuteState;

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public ReactiveCollection<OnvifNetworkCameraProfile> NetworkCameraProfileItems { get; }

        [NullObjectValidation]
        public ReactiveProperty<OnvifNetworkCameraProfile> SelectedNetworkCameraProfile { get; }

        public ReactiveCommand NetworkCameraProfileItemSelectionChanged { get; }

        public ReactiveProperty<int> FrameRateLimit { get; }

        public ReactiveProperty<int> BitRateLimit { get; }

        public AsyncReactiveCommand UpMoveCommand { get; }

        public AsyncReactiveCommand DownMoveCommand { get; }

        public AsyncReactiveCommand LeftMoveCommand { get; }

        public AsyncReactiveCommand RightMoveCommand { get; }

        public AsyncReactiveCommand ZoomInCommand { get; }

        public AsyncReactiveCommand ZoomOutCommand { get; }

        private void OnNetworkCameraProfileItemSelectionChanged()
        {
            if (SelectedNetworkCameraProfile.Value != null)
            {
                FrameRateLimit.Value = SelectedNetworkCameraProfile.Value.FrameRateLimit;
                BitRateLimit.Value = SelectedNetworkCameraProfile.Value.BitRateLimit;

                _networkCameraSettingService.StartCapture();
            }
        }

        private async Task OnUpMoveCommand()
        {
            await _networkCameraSettingService.MoveAsync(PtzDirection.UpMove);
        }

        private async Task OnDownMoveCommand()
        {
            await _networkCameraSettingService.MoveAsync(PtzDirection.DownMove);
        }

        private async Task OnLeftMoveCommand()
        {
            await _networkCameraSettingService.MoveAsync(PtzDirection.LeftMove);
        }

        private async Task OnRightMoveCommand()
        {
            await _networkCameraSettingService.MoveAsync(PtzDirection.RightMove);
        }

        private async Task OnZoomInCommand()
        {
            await _networkCameraSettingService.MoveAsync(PtzDirection.ZoomIn);
        }

        private async Task OnZoomOutCommand()
        {
            await _networkCameraSettingService.MoveAsync(PtzDirection.ZoomOut);
        }
    }
}
