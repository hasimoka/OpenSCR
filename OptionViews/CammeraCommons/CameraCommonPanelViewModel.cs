using HalationGhost.WinApps;
using OptionViews.AdvancedCameraSettings;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Prism.Ioc;
using OpenSCRLib;
using OptionViews.Services;

namespace OptionViews.CammeraCommons
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
            // DBにカメラ設定を保存する
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            var loadedSettings = dbAccessor.FindCameraSettings();

            foreach (var loadedSetting in loadedSettings)
            {
                var setting = new CommonCameraSetting(loadedSetting, this.regionManager, this.container, this.commonCameraSettingService);
                this.CameraSettings.Add(setting);
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
