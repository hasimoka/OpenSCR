using HalationGhost.WinApps;
using OpenSCRLib;
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
        public ReactiveProperty<string> CameraLoginAnnouncement { get; }

        public ReactiveCommand LogoutCameraClick { get; }

        private IContainerProvider container = null;

        public CameraLogoutSettingPanelViewModel(IRegionManager regionMan, IContainerProvider injectionContainer) : base(regionMan)
        {
            this.container = injectionContainer;

            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            var cameraLoginInfo = dbAccessor.GetCameraLoginInfo();

            this.CameraLoginAnnouncement = new ReactiveProperty<string>(this.GetCameraLoginMessage(cameraLoginInfo.Name))
                .AddTo(this.disposable);
            
            this.LogoutCameraClick = new ReactiveCommand()
                .WithSubscribe(() => this.onLogoutCameraClick())
                .AddTo(this.disposable);
        }

        private void onLogoutCameraClick()
        {
            // ログイン状態を「ログアウト」に変更する
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            var camearLoginInfo = dbAccessor.GetCameraLoginInfo();
            camearLoginInfo.IsLoggedIn = false;

            dbAccessor.SetCameraLoginInfo(camearLoginInfo);

            this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLoginSettingPanel");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            var camearLoginInfo = dbAccessor.GetCameraLoginInfo();

            this.CameraLoginAnnouncement.Value = this.GetCameraLoginMessage(camearLoginInfo.Name);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { return; }

        private string GetCameraLoginMessage(string name)
        {
            return $"Logged in as \"{name}\"";
        }
    }
}
