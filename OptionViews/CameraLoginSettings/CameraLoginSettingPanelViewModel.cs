using HalationGhost.WinApps;
using OpenSCRLib;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.CameraLoginSettings
{
    class CameraLoginSettingPanelViewModel : HalationGhostViewModelBase
    {
        public ReactiveProperty<string> CameraLoginName { get; }

        public ReactiveProperty<string> CameraPassword { get; }

        public ReactiveCommand LoginCameraClick { get; }

        private IContainerProvider container = null;

        public CameraLoginSettingPanelViewModel(IRegionManager regionMan, IContainerProvider injectionContainer) : base(regionMan)
        {
            this.container = injectionContainer;
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();

            var cameraLoginInfo = dbAccessor.GetCameraLoginInfo();
            var name = string.Empty;
            var password = string.Empty;

            if (cameraLoginInfo != null)
            {
                name = cameraLoginInfo.Name;
                password = cameraLoginInfo.Password;
            }

            this.CameraLoginName = new ReactiveProperty<string>(name)
                .AddTo(this.disposable);

            this.CameraPassword = new ReactiveProperty<string>(password)
                .AddTo(this.disposable);

            this.LoginCameraClick = new ReactiveCommand()
                .WithSubscribe(() => this.onLoginCameraClick())
                .AddTo(this.disposable);
        }

        private void onLoginCameraClick()
        {
            // ログイン状態を「ログイン」に変更する
            var camearLoginInfo = new CameraLoginInfo();
            camearLoginInfo.IsLoggedIn = true;
            camearLoginInfo.Name = this.CameraLoginName.Value;
            camearLoginInfo.Password = this.CameraPassword.Value;

            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            dbAccessor.SetCameraLoginInfo(camearLoginInfo);

            this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLogoutSettingPanel");
        }
    }
}
