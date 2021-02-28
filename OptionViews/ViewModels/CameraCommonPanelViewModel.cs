using HalationGhost.WinApps;
using OpenSCRLib;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using OptionViews.Models;

namespace OptionViews.ViewModels
{
    class CameraCommonPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        private ICommonCameraSettingService commonCameraSettingService;

        public CameraCommonPanelViewModel(IRegionManager regionMan, IContainerProvider injectionContainer, ICommonCameraSettingService commonCameraSettingService) : base(regionMan)
        {
            this.container = injectionContainer;
            this.commonCameraSettingService = commonCameraSettingService;

            this.ReturnOptionCommonPanelClick = new ReactiveCommand()
                .WithSubscribe(() => this.onReturnOptionCommonPanelClick())
                .AddTo(this.disposable);

            this.AddCameraSettingClick = new ReactiveCommand()
                .WithSubscribe(() => this.onAddCameraSettingClick())
                .AddTo(this.disposable);

            this.CameraSettings = this.commonCameraSettingService.CameraSettings
                .AddTo(this.disposable);
        }

        public ReactiveCommand ReturnOptionCommonPanelClick { get; }

        public ReactiveCommand AddCameraSettingClick { get; }

        public ReactiveCollection<CommonCameraSetting> CameraSettings { get; }



        private void onReturnOptionCommonPanelClick()
        {
            Console.WriteLine("Call onReturnOptionCommonPanelClick() method.");
            this.regionManager.RequestNavigate("ContentRegion", "OptionCommonPanel");
        }

        private void onAddCameraSettingClick()
        {
            Console.WriteLine("Call onAddCameraSettingClick() method.");

            var parameter = new NavigationParameters();
            parameter.Add("CameraSetting", null);
            this.regionManager.RequestNavigate("ContentRegion", "AdvancedCameraSettingPanel", parameter);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.CameraSettings.Clear();

            // DBからカメラ設定を取得する
            var dbAccessor = this.container.Resolve<DatabaseAccessor>();
            var loadedSettings = dbAccessor.FindCameraSettings();

            foreach (var loadedSetting in loadedSettings)
            {
                var setting = this.container.Resolve<CommonCameraSetting>((typeof(CameraSetting), loadedSetting));
                this.CameraSettings.Add(setting);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
