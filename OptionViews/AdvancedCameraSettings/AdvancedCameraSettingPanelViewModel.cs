using HalationGhost.WinApps;
using OpenSCRLib;
using OptionViews.Models;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace OptionViews.AdvancedCameraSettings
{
    class AdvancedCameraSettingPanelViewModel : HalationGhostViewModelBase
    {
        private IContainerProvider container;

        public AdvancedCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider) : base(regionMan)
        {
            this.container = containerProvider;

            this.CameraTypeItems = new ReactiveCollection<CameraTypeItem>() {
                new CameraTypeItem() { Name="IP Camera(ONVIF)", Type=CameraType.IpCamera },
                new CameraTypeItem() { Name="USB Camera", Type=CameraType.UsbCamera }
            };
            this.CameraTypeItems.AddTo(this.disposable);

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

        public ReactiveCollection<CameraTypeItem> CameraTypeItems { get; }

        public ReactiveProperty<CameraTypeItem> SelectedCameraType { get; }

        public ReactiveCommand RefreshDeviceListClick { get; }

        public ReactiveCommand ChangeSelectedCameraType { get; }

        public void SetIpCameraSetting(List<IpCameraProfile> profiles)
        {
            if (this.SelectedCameraType.Value.Type == CameraType.IpCamera)
            {
                Console.WriteLine($"profiles: {profiles}");
                var view = this.regionManager.Regions["CameraSettingRegion"].ActiveViews.FirstOrDefault() as IpCameraSettingPanel;
                var viewModel = view.DataContext as IpCameraSettingPanelViewModel;

                viewModel.SetSetting(profiles);
            }
        }

        private void onRefreshDeviceListClick()
        {
            if (this.SelectedCameraType.Value.Type == CameraType.IpCamera)
            {
                var view = this.regionManager.Regions["CameraListRegion"].ActiveViews.FirstOrDefault() as IpCameraListPanel;
                var viewModel = view.DataContext as IpCameraListPanelViewModel;
                viewModel.RefreshCameraList();
            }
            else if (this.SelectedCameraType.Value.Type == CameraType.UsbCamera)
            {
                var view = this.regionManager.Regions["CameraListRegion"].ActiveViews.FirstOrDefault() as UsbCameraListPanel;
                var viewModel = view.DataContext as UsbCameraListPanelViewModel; ;
                viewModel.RefreshCameraList();
            }
        }

        private void onChangeSelectedCameraType()
        {
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
