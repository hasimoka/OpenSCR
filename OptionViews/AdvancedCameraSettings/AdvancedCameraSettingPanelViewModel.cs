using HalationGhost.WinApps;
using MainWindowServices;
using Mictlanix.DotNet.Onvif.Common;
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
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OptionViews.AdvancedCameraSettings
{
    class AdvancedCameraSettingPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        private IMainWindowService mainWindowService;

        private INetworkCameraSettingService networkCameraSettingService;

        private IUsbCameraSettingService usbCameraSettingService;

        public AdvancedCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService, INetworkCameraSettingService networkCameraSettingService, IUsbCameraSettingService usbCameraSettingService) : base(regionMan)
        {
            this.container = containerProvider;
            this.mainWindowService = windowService;
            this.networkCameraSettingService = networkCameraSettingService;
            this.usbCameraSettingService = usbCameraSettingService;

            this.IsProgressRingDialogOpen = this.mainWindowService.IsProgressRingDialogOpen
                .AddTo(this.disposable);

            this.CameraChannel = new ReactivePropertySlim<int>()
                .AddTo(this.disposable);

            this.CameraName = new ReactivePropertySlim<string>()
                .AddTo(this.disposable);

            this.CameraTypeItems = new ReactiveCollection<CameraTypeItem>() {
                new CameraTypeItem() { Name="IP Camera(ONVIF)", Type=CameraType.NetworkCamera },
                new CameraTypeItem() { Name="USB Camera", Type=CameraType.UsbCamera }
            };
            this.CameraTypeItems.AddTo(this.disposable);

            this.ChangeSelectedCameraType = new ReactiveCommand()
                .WithSubscribe(() => this.onChangeSelectedCameraType())
                .AddTo(this.disposable);

            this.SelectedCameraType = new ReactiveProperty<CameraTypeItem>(this.CameraTypeItems.First())
                .AddTo(this.disposable);

            this.RefreshDeviceListClick = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    this.IsProgressRingDialogOpen.Value = true;
                    await this.onRefreshDeviceListClick();
                    this.IsProgressRingDialogOpen.Value = false;
                })
                .AddTo(this.disposable);

            this.AcceptCommand = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    this.IsProgressRingDialogOpen.Value = true;
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    await this.onAcceptButtonClickAsync();
                    this.IsProgressRingDialogOpen.Value = false;


                    // 1つ前の画面に戻る
                    this.regionManager.RequestNavigate("ContentRegion", "CameraCommonPanel");
                })
                .AddTo(this.disposable);

            this.CancelCommand = new ReactiveCommand()
                .WithSubscribe(() => this.onCancelButtonClick())
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<bool> IsProgressRingDialogOpen { get; set; }

        public ReactivePropertySlim<int> CameraChannel { get; }

        public ReactivePropertySlim<string> CameraName { get; }

        public ReactiveCollection<CameraTypeItem> CameraTypeItems { get; }

        public ReactiveProperty<CameraTypeItem> SelectedCameraType { get; }

        public AsyncReactiveCommand RefreshDeviceListClick { get; }

        public AsyncReactiveCommand RefreshNetowrkCameraPanelDevliceListClick { get; }

        public AsyncReactiveCommand RefreshUsbCameraPanelDevliceListClick { get; }

        public ReactiveCommand ChangeSelectedCameraType { get; }

        public AsyncReactiveCommand AcceptCommand { get; }

        public ReactiveCommand CancelCommand { get; }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var cameraSetting = navigationContext.Parameters["CameraSetting"] as CameraSetting;
            if (cameraSetting != null)
            {
                this.CameraChannel.Value = cameraSetting.CameraChannel;

                // 設定を画面に反映させる
                this.CameraName.Value = cameraSetting.CameraName;

                if (cameraSetting.NetworkCameraSetting != null)
                {
                    // ネットワークカメラ設定の場合
                    foreach (var item in CameraTypeItems)
                    {
                        if (item.Type == CameraType.NetworkCamera)
                        {
                            this.SelectedCameraType.Value = item;
                        }
                    }

                    this.networkCameraSettingService.UserName.Value = cameraSetting.NetworkCameraSetting.UserName;
                    this.networkCameraSettingService.Password.Value = cameraSetting.NetworkCameraSetting.Password;
                    this.networkCameraSettingService.IsLoggedIn.Value = cameraSetting.NetworkCameraSetting.IsLoggedIn;
                    if (cameraSetting.NetworkCameraSetting.IsLoggedIn)
                    {
                        this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLogoutSettingPanel");
                    }
                    else
                    {
                        this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLoginSettingPanel");
                    }

                    var ipCameraListPanelParameter = new NavigationParameters();
                    ipCameraListPanelParameter.Add("NetworkCameraSetting", cameraSetting.NetworkCameraSetting);
                    this.regionManager.RequestNavigate("CameraListRegion", "IpCameraListPanel", ipCameraListPanelParameter);
                    this.regionManager.RequestNavigate("CameraSettingRegion", "IpCameraSettingPanel");
                }
                else if (cameraSetting.UsbCameraSetting != null)
                {
                    // USBカメラ設定の場合
                    foreach (var item in CameraTypeItems)
                    {
                        if (item.Type == CameraType.UsbCamera)
                        {
                            this.SelectedCameraType.Value = item;
                        }
                    }

                    var usbCameraListPanelParameter = new NavigationParameters();
                    usbCameraListPanelParameter.Add("UsbCameraSetting", cameraSetting.UsbCameraSetting);
                    this.regionManager.RequestNavigate("CameraListRegion", "UsbCameraListPanel", usbCameraListPanelParameter);
                    this.regionManager.RequestNavigate("CameraSettingRegion", "UsbCameraSettingPanel");
                }
            }
            else
            {
                // 新規作成の場合
                var dbAccessor = this.container.Resolve<DatabaseAccesser>();
                this.CameraChannel.Value = dbAccessor.GetNextCameraChannel();

                this.regionManager.RequestNavigate("CameraListRegion", "IpCameraListPanel");
                this.regionManager.RequestNavigate("CameraSettingRegion", "IpCameraSettingPanel");
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }


        // 拡張メソッド
        private static string GetVideoEncodingName(VideoEncoding encoding)
        { 
            string[] videoEncodingNames = { "MJPEG", "MPEG4", "H264", "H25" };
            return videoEncodingNames[(int)encoding];
        }

        private async Task onRefreshDeviceListClick()
        {
            if (this.SelectedCameraType.Value.Type == CameraType.NetworkCamera)
            {
                this.networkCameraSettingService.CanSelectionChangedCameraDeviceListCommand.Value = false;
                await this.networkCameraSettingService.RefreshCameraList();
                this.networkCameraSettingService.CanSelectionChangedCameraDeviceListCommand.Value = true;
            }
            else if (this.SelectedCameraType.Value.Type == CameraType.UsbCamera)
            {
                this.usbCameraSettingService.CanSelectionChangedCameraDeviceListCommand.Value = false;
                await this.usbCameraSettingService.RefreshCameraList();
                this.usbCameraSettingService.CanSelectionChangedCameraDeviceListCommand.Value = true;
            }
        }

        private void onChangeSelectedCameraType()
        {
            // 選択されたアイテムによって、画面構成を変更する
            if (this.SelectedCameraType.Value != null)
            {
                if (this.SelectedCameraType.Value.Type == CameraType.NetworkCamera)
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

        private async Task onAcceptButtonClickAsync()
        {
            try
            {
                if (this.SelectedCameraType.Value.Type == CameraType.NetworkCamera)
                {
                    // ネットワークカメラへ入力された設定された値をONVIFを通して反映させる
                    await this.networkCameraSettingService.SetCameraDeviceInfoAsync();
                }

                var dbAccessor = this.container.Resolve<DatabaseAccesser>();
                var setting = dbAccessor.FindCameraSetting(this.CameraChannel.Value);
                if (setting == null)
                    setting = new CameraSetting();
                setting.CameraChannel = this.CameraChannel.Value;
                setting.CameraName = this.CameraName.Value;

                if (this.SelectedCameraType.Value.Type == CameraType.NetworkCamera)
                {
                    // ネットワークカメラ用設定を保存する
                    var networkCameraSetting = new NetworkCameraSetting();

                    networkCameraSetting.UserName = this.networkCameraSettingService.UserName.Value;
                    networkCameraSetting.Password = this.networkCameraSettingService.Password.Value;
                    networkCameraSetting.IsLoggedIn = this.networkCameraSettingService.IsLoggedIn.Value;

                    if (this.networkCameraSettingService.CameraDeviceSelectedItem.Value == null)
                        throw new ValidationException();
                    var deviceListItem = this.networkCameraSettingService.CameraDeviceSelectedItem.Value;
                    networkCameraSetting.ProductName = deviceListItem.DeviceName.Value;
                    networkCameraSetting.IpAddress = deviceListItem.IpAddress.Value;

                    if (this.networkCameraSettingService.SelectedNetworkCameraProfile.Value == null)
                        throw new ValidationException();
                    var profile = this.networkCameraSettingService.SelectedNetworkCameraProfile.Value;
                    networkCameraSetting.ProfileToken = profile.ProfileToken;
                    networkCameraSetting.StreamUri = profile.StreamUri;
                    networkCameraSetting.Encoding = GetVideoEncodingName(profile.Encoding);
                    networkCameraSetting.CameraHeight = profile.VideoHeight;
                    networkCameraSetting.CameraWidth = profile.VideoWidth;

                    setting.NetworkCameraSetting = networkCameraSetting;
                }
                else if (this.SelectedCameraType.Value.Type == CameraType.UsbCamera)
                {
                    // USBカメラ用設定を保存する
                    var usbCameraSetting = new UsbCameraSetting();

                    usbCameraSetting.DevicePath = this.usbCameraSettingService.CameraDeviceSelectedItem.Value.DevicePath;
                    usbCameraSetting.CameraWidth = this.usbCameraSettingService.SelectedUsbCameraVideoInfo.Value.Width;
                    usbCameraSetting.CameraHeight = this.usbCameraSettingService.SelectedUsbCameraVideoInfo.Value.Height;
                    usbCameraSetting.FrameRate = this.usbCameraSettingService.SelectedUsbCameraVideoInfo.Value.BitCount;

                    setting.UsbCameraSetting = usbCameraSetting;
                }

                // DBにカメラ設定を保存する
                dbAccessor.UpdateOrInsertCameraSetting(setting);

                // 入力値をクリアする
                this.clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void onCancelButtonClick()
        {
            // 入力値をクリアする
            this.clear();

            // 1つ前の画面に戻る
            this.regionManager.RequestNavigate("ContentRegion", "CameraCommonPanel");
        }

        private void clear()
        {
            this.CameraChannel.Value = 0;
            this.CameraName.Value = string.Empty;
            this.SelectedCameraType.Value = this.CameraTypeItems.FirstOrDefault();

            this.networkCameraSettingService.Clear();
            this.usbCameraSettingService.Clear();
        }
    }
}
