using HalationGhost.WinApps;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace OptionViews.ViewModels
{
    class CameraLoginSettingPanelViewModel : HalationGhostViewModelBase
    {
        private IContainerProvider container;

        private INetworkCameraSettingService networkCameraSettingService;

        public CameraLoginSettingPanelViewModel(IRegionManager regionMan, IContainerProvider injectionContainer, INetworkCameraSettingService networkCameraSettingService) : base(regionMan)
        {
            this.container = injectionContainer;
            this.networkCameraSettingService = networkCameraSettingService;

            this.CameraLoginName = this.networkCameraSettingService.UserName
                .AddTo(this.disposable);

            this.CameraPassword = this.networkCameraSettingService.Password
                .AddTo(this.disposable);

            this.LoginCameraClick = new ReactiveCommand()
                .WithSubscribe(() => this.onLoginCameraClick())
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<string> CameraLoginName { get; }

        public ReactivePropertySlim<string> CameraPassword { get; }

        public ReactiveCommand LoginCameraClick { get; }

        private void onLoginCameraClick()
        {
            // ログイン状態を「ログイン」に変更する
            this.networkCameraSettingService.IsLoggedIn.Value = true;
            this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLogoutSettingPanel");
        }
    }
}
