using HalationGhost.WinApps;
using OptionViews.Models;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;

namespace OptionViews.AdvancedCameraSettings
{
    class AdvancedCameraSettingPanelViewModel : HalationGhostViewModelBase
    {
        public ReactiveCollection<CameraTypeItem> CameraTypeItems { get; }

        public ReactiveProperty<CameraTypeItem> SelectedCameraType { get; }

        public ReactiveCommand RefreshDeviceListClick { get; }

        public ReactiveCommand ChangeSelectedCameraType { get; }

        public ReactiveCollection<OnvifProfileViewModel> OnvifProfileListSource { get; }

        public ReactiveProperty<OnvifProfileViewModel> OnvifProfileSelectedItem { get; set; }

        private IContainerProvider container;

        public AdvancedCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider) : base(regionMan)
        {
            this.container = containerProvider;

            this.CameraTypeItems = new ReactiveCollection<CameraTypeItem>() {
                new CameraTypeItem() { Name="IP Camera(ONVIF)", Type=CameraType.IpCamera },
                new CameraTypeItem() { Name="USB Camera", Type=CameraType.UsbCamera }
            };

            this.ChangeSelectedCameraType = new ReactiveCommand()
                .WithSubscribe(() => this.onChangeSelectedCameraType())
                .AddTo(this.disposable);

            this.SelectedCameraType = new ReactiveProperty<CameraTypeItem>(this.CameraTypeItems.First())
                .AddTo(this.disposable);

            this.RefreshDeviceListClick = new ReactiveCommand()
                .WithSubscribe(() => this.onRefreshDeviceListClick())
                .AddTo(this.disposable);

            regionMan.RegisterViewWithRegion("CameraListRegion", typeof(IpCameraListPanel));
            regionMan.RegisterViewWithRegion("CameraSettingRegion", typeof(IpCameraSettingPanel));
        }

        private void onRefreshDeviceListClick()
        {
            Console.WriteLine("Call onRefreshDeviceListClick() method.");

            if (this.SelectedCameraType.Value.Type == CameraType.IpCamera)
            {
                var view = this.regionManager.Regions["CameraListRegion"].ActiveViews.FirstOrDefault() as IpCameraListPanel;
                Console.WriteLine($"View: {view.DataContext}");
            }
            else if (this.SelectedCameraType.Value.Type == CameraType.UsbCamera)
            {
                var view = this.regionManager.Regions["CameraListRegion"].ActiveViews.FirstOrDefault() as UsbCameraListPanel;
                Console.WriteLine($"View: {view.DataContext}");
            }
        }

        private void onChangeSelectedCameraType()
        {
            Console.WriteLine("Call onChangeSelectedCameraType() method.");

            // 選択されたアイテムによって、画面構成を変更する
            if (this.SelectedCameraType.Value != null)
            {
                if (this.SelectedCameraType.Value.Type == CameraType.IpCamera)
                {
                    // IPカメラ(ONVIF)向けの画面構成に変更する
                    this.regionManager.RequestNavigate("CameraListRegion", "IpCameraListPanel");
                    this.regionManager.RequestNavigate("CameraSettingRegion", "IpCameraSettingPanel");
                }
                else if (this.SelectedCameraType.Value.Type == CameraType.UsbCamera)
                {
                    // USBカメラ向けの画面構成に変更する
                    this.regionManager.RequestNavigate("CameraListRegion", "UsbCameraListPanel");
                    this.regionManager.RequestNavigate("CameraSettingRegion", "UsbCameraSettingPanel");
                }
            }

        }
    }
}
