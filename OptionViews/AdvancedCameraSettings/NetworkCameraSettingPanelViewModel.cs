using HalationGhost.WinApps;
using OnvifNetworkCameraClient.Models;
using OpenSCRLib;
using OptionViews.Models;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OptionViews.AdvancedCameraSettings
{
    class NetworkCameraSettingPanelViewModel : HalationGhostViewModelBase
    {
        private IContainerProvider container;

        private INetworkCameraSettingService networkCameraSettingService;

        public NetworkCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, INetworkCameraSettingService cameraSettingService) : base(regionMan)
        {
            this.container = containerProvider;
            this.networkCameraSettingService = cameraSettingService;

            this.SharedCanExecuteState = new ReactivePropertySlim<bool>(true)
                .AddTo(this.disposable);

            this.FrameImage = this.networkCameraSettingService.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.disposable);

            this.NetworkCameraProfileItems = this.networkCameraSettingService.NetworkCameraProfileItems
                .AddTo(this.disposable);

            this.SelectedNetworkCameraProfile = this.networkCameraSettingService.SelectedNetworkCameraProfile
                .SetValidateAttribute(()=>SelectedNetworkCameraProfile)
                .AddTo(this.disposable);

            this.NetworkCameraProfileItemSelectionChanged = new ReactiveCommand()
                .WithSubscribe(() => this.onNetworkCameraProfileItemSelectionChanged())
                .AddTo(this.disposable);

            this.FrameRateLimit = this.networkCameraSettingService.FrameRateLimit
                .AddTo(this.disposable);

            this.BitrateLimit = this.networkCameraSettingService.BitRateLimit
                .AddTo(this.disposable);

            this.UpMoveCommand = this.SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(this.SharedCanExecuteState)
                .WithSubscribe(onUpMoveCommand)
                .AddTo(this.disposable);

            this.DownMoveCommand = this.SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(this.SharedCanExecuteState)
                .WithSubscribe(onDownMoveCommand)
                .AddTo(this.disposable);

            this.LeftMoveCommand = this.SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(this.SharedCanExecuteState)
                .WithSubscribe(onLeftMoveCommand)
                .AddTo(this.disposable);

            this.RightMoveCommand = this.SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(this.SharedCanExecuteState)
                .WithSubscribe(onRightMoveCommand)
                .AddTo(this.disposable);

            this.ZoomInCommand = this.SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(this.SharedCanExecuteState)
                .WithSubscribe(onZoomInCommand)
                .AddTo(this.disposable);

            this.ZoomOutCommand = this.SelectedNetworkCameraProfile
                .ObserveHasErrors
                .Inverse()
                .ToAsyncReactiveCommand(this.SharedCanExecuteState)
                .WithSubscribe(onZoomOutCommand)
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<bool> SharedCanExecuteState;

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public ReactiveCollection<OnvifNetworkCameraProfile> NetworkCameraProfileItems { get; }

        [NullObjectValidation]
        public ReactiveProperty<OnvifNetworkCameraProfile> SelectedNetworkCameraProfile { get; }

        public ReactiveCommand NetworkCameraProfileItemSelectionChanged { get; }

        public ReactiveProperty<int> FrameRateLimit { get; }

        public ReactiveProperty<int> BitrateLimit { get; }

        public AsyncReactiveCommand UpMoveCommand { get; }

        public AsyncReactiveCommand DownMoveCommand { get; }

        public AsyncReactiveCommand LeftMoveCommand { get; }

        public AsyncReactiveCommand RightMoveCommand { get; }

        public AsyncReactiveCommand ZoomInCommand { get; }

        public AsyncReactiveCommand ZoomOutCommand { get; }

        private void onNetworkCameraProfileItemSelectionChanged()
        {
            if (this.SelectedNetworkCameraProfile.Value != null)
            {
                this.FrameRateLimit.Value = this.SelectedNetworkCameraProfile.Value.FrameRateLimit;
                this.BitrateLimit.Value = this.SelectedNetworkCameraProfile.Value.BitrateLimite;

                this.networkCameraSettingService.StartCapture();
            }
        }

        private async Task onUpMoveCommand()
        {
            await this.networkCameraSettingService.MoveAsync(PtzDirection.UpMove);
        }

        private async Task onDownMoveCommand()
        {
            await this.networkCameraSettingService.MoveAsync(PtzDirection.DownMove);
        }

        private async Task onLeftMoveCommand()
        {
            await this.networkCameraSettingService.MoveAsync(PtzDirection.LeftMove);
        }

        private async Task onRightMoveCommand()
        {
            await this.networkCameraSettingService.MoveAsync(PtzDirection.RightMove);
        }

        private async Task onZoomInCommand()
        {
            await this.networkCameraSettingService.MoveAsync(PtzDirection.ZoomIn);
        }

        private async Task onZoomOutCommand()
        {
            await this.networkCameraSettingService.MoveAsync(PtzDirection.ZoomOut);
        }
    }
}
