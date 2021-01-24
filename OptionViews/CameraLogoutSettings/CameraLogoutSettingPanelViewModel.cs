using HalationGhost.WinApps;
using OpenSCRLib;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.CameraLogoutSettings
{
    class CameraLogoutSettingPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        private INetworkCameraSettingService networkCameraSettingServer;

        public CameraLogoutSettingPanelViewModel(IRegionManager regionMan, IContainerProvider injectionContainer, INetworkCameraSettingService networkCameraSettingServer) : base(regionMan)
        {
            this.container = injectionContainer;
            this.networkCameraSettingServer = networkCameraSettingServer;

            this.CameraLoginAnnouncement = new ReactiveProperty<string>(this.GetCameraLoginMessage(this.networkCameraSettingServer.UserName.Value))
                .AddTo(this.disposable);
            
            this.LogoutCameraClick = new ReactiveCommand()
                .WithSubscribe(() => this.onLogoutCameraClick())
                .AddTo(this.disposable);
        }

        public ReactiveProperty<string> CameraLoginAnnouncement { get; }

        public ReactiveCommand LogoutCameraClick { get; }

        private void onLogoutCameraClick()
        {
            // ログイン状態を「ログアウト」に変更する
            this.networkCameraSettingServer.IsLoggedIn.Value = false;
            this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLoginSettingPanel");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.CameraLoginAnnouncement.Value = this.GetCameraLoginMessage(this.networkCameraSettingServer.UserName.Value);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { return; }

        private string GetCameraLoginMessage(string name)
        {
            return $"Logged in as \"{name}\"";
        }
    }
}
