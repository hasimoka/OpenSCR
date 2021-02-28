using CameraClient.Models;
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
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OptionViews.ViewModels
{
    class AdvancedCameraSettingPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private readonly IContainerProvider _container;

        private readonly IMainWindowService _mainWindowService;

        private readonly INetworkCameraSettingService _networkCameraSettingService;

        private readonly IUsbCameraSettingService _usbCameraSettingService;

        public AdvancedCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService, INetworkCameraSettingService networkCameraSettingService, IUsbCameraSettingService usbCameraSettingService) : base(regionMan)
        {
            _container = containerProvider;
            _mainWindowService = windowService;
            _networkCameraSettingService = networkCameraSettingService;
            _usbCameraSettingService = usbCameraSettingService;

            IsProgressRingDialogOpen = _mainWindowService.IsProgressRingDialogOpen
                .AddTo(disposable);

            CameraChannel = new ReactivePropertySlim<int>()
                .AddTo(disposable);

            CameraName = new ReactivePropertySlim<string>()
                .AddTo(disposable);

            CameraTypeItems = new ReactiveCollection<CameraTypeItem>() {
                new CameraTypeItem() { Name="IP Camera(ONVIF)", Type=CameraType.NetworkCamera },
                new CameraTypeItem() { Name="USB Camera", Type=CameraType.UsbCamera }
            };
            CameraTypeItems.AddTo(disposable);

            ChangeSelectedCameraType = new ReactiveCommand()
                .WithSubscribe(() => OnChangeSelectedCameraType())
                .AddTo(disposable);

            SelectedCameraType = new ReactiveProperty<CameraTypeItem>(CameraTypeItems.First())
                .AddTo(disposable);

            RefreshDeviceListClick = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    IsProgressRingDialogOpen.Value = true;
                    await OnRefreshDeviceListClick();
                    IsProgressRingDialogOpen.Value = false;
                })
                .AddTo(disposable);

            AcceptCommand = new AsyncReactiveCommand()
                .WithSubscribe(async () =>
                {
                    IsProgressRingDialogOpen.Value = true;
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    await OnAcceptButtonClickAsync();
                    IsProgressRingDialogOpen.Value = false;


                    // 1つ前の画面に戻る
                    regionManager.RequestNavigate("ContentRegion", "CameraCommonPanel");
                })
                .AddTo(disposable);

            CancelCommand = new ReactiveCommand()
                .WithSubscribe(() => OnCancelButtonClick())
                .AddTo(disposable);
        }

        public ReactivePropertySlim<bool> IsProgressRingDialogOpen { get; set; }

        public ReactivePropertySlim<int> CameraChannel { get; }

        public ReactivePropertySlim<string> CameraName { get; }

        public ReactiveCollection<CameraTypeItem> CameraTypeItems { get; }

        public ReactiveProperty<CameraTypeItem> SelectedCameraType { get; }

        public AsyncReactiveCommand RefreshDeviceListClick { get; }

        public AsyncReactiveCommand RefreshNetworkCameraPanelDeviceListClick { get; }

        public AsyncReactiveCommand RefreshUsbCameraPanelDeviceListClick { get; }

        public ReactiveCommand ChangeSelectedCameraType { get; }

        public AsyncReactiveCommand AcceptCommand { get; }

        public ReactiveCommand CancelCommand { get; }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["CameraSetting"] is CameraSetting cameraSetting)
            {
                CameraChannel.Value = cameraSetting.CameraChannel;

                // 設定を画面に反映させる
                CameraName.Value = cameraSetting.CameraName;

                if (cameraSetting.NetworkCameraSettings != null)
                {
                    // ネットワークカメラ設定の場合
                    foreach (var item in CameraTypeItems)
                    {
                        if (item.Type == CameraType.NetworkCamera)
                        {
                            SelectedCameraType.Value = item;
                        }
                    }

                    _networkCameraSettingService.UserName.Value = cameraSetting.NetworkCameraSettings.UserName;
                    _networkCameraSettingService.Password.Value = cameraSetting.NetworkCameraSettings.Password;
                    _networkCameraSettingService.IsLoggedIn.Value = cameraSetting.NetworkCameraSettings.IsLoggedIn;
                    if (cameraSetting.NetworkCameraSettings.IsLoggedIn)
                    {
                        regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLogoutSettingPanel");
                    }
                    else
                    {
                        regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLoginSettingPanel");
                    }

                    var ipCameraListPanelParameter = new NavigationParameters();
                    ipCameraListPanelParameter.Add("NetworkCameraSettings", cameraSetting.NetworkCameraSettings);
                    regionManager.RequestNavigate("CameraListRegion", "IpCameraListPanel", ipCameraListPanelParameter);
                    regionManager.RequestNavigate("CameraSettingRegion", "IpCameraSettingPanel");
                }
                else if (cameraSetting.UsbCameraSettings != null)
                {
                    // USBカメラ設定の場合
                    foreach (var item in CameraTypeItems)
                    {
                        if (item.Type == CameraType.UsbCamera)
                        {
                            SelectedCameraType.Value = item;
                        }
                    }

                    var usbCameraListPanelParameter = new NavigationParameters();
                    usbCameraListPanelParameter.Add("UsbCameraSettings", cameraSetting.UsbCameraSettings);
                    regionManager.RequestNavigate("CameraListRegion", "UsbCameraListPanel", usbCameraListPanelParameter);
                    regionManager.RequestNavigate("CameraSettingRegion", "UsbCameraSettingPanel");
                }
            }
            else
            {
                // 新規作成の場合
                var dbAccessor = _container.Resolve<DatabaseAccessor>();
                CameraChannel.Value = dbAccessor.GetNextCameraChannel();

                regionManager.RequestNavigate("CameraListRegion", "IpCameraListPanel");
                regionManager.RequestNavigate("CameraSettingRegion", "IpCameraSettingPanel");
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }


        // 拡張メソッド
        private static string GetVideoEncodingName(VideoEncoding encoding)
        { 
            string[] videoEncodingNames = { "MJPEG", "MPEG4", "H264", "H265" };
            return videoEncodingNames[(int)encoding];
        }

        private async Task OnRefreshDeviceListClick()
        {
            if (SelectedCameraType.Value.Type == CameraType.NetworkCamera)
            {
                _networkCameraSettingService.CanSelectionChangedCameraDeviceListCommand.Value = false;
                await _networkCameraSettingService.RefreshCameraList();
                _networkCameraSettingService.CanSelectionChangedCameraDeviceListCommand.Value = true;
            }
            else if (SelectedCameraType.Value.Type == CameraType.UsbCamera)
            {
                _usbCameraSettingService.CanSelectionChangedCameraDeviceListCommand.Value = false;
                await _usbCameraSettingService.RefreshCameraList();
                _usbCameraSettingService.CanSelectionChangedCameraDeviceListCommand.Value = true;
            }
        }

        private void OnChangeSelectedCameraType()
        {
            // 選択されたアイテムによって、画面構成を変更する
            if (SelectedCameraType.Value != null)
            {
                if (SelectedCameraType.Value.Type == CameraType.NetworkCamera)
                {
                    // IPカメラ(ONVIF)向けの画面構成に変更する
                    regionManager.RequestNavigate("CameraListRegion", "IpCameraListPanel");
                    regionManager.RequestNavigate("CameraSettingRegion", "IpCameraSettingPanel");
                }
                else if (SelectedCameraType.Value.Type == CameraType.UsbCamera)
                {
                    // USBカメラ向けの画面構成に変更する
                    regionManager.RequestNavigate("CameraListRegion", "UsbCameraListPanel");
                    regionManager.RequestNavigate("CameraSettingRegion", "UsbCameraSettingPanel");
                }
            }
        }

        private async Task OnAcceptButtonClickAsync()
        {
            try
            {
                if (SelectedCameraType.Value.Type == CameraType.NetworkCamera)
                {
                    // ネットワークカメラへ入力された設定された値をONVIFを通して反映させる
                    await _networkCameraSettingService.SetCameraDeviceInfoAsync();
                }

                var dbAccessor = _container.Resolve<DatabaseAccessor>();
                var setting = dbAccessor.FindCameraSetting(CameraChannel.Value);
                if (setting == null)
                    setting = new CameraSetting();
                setting.CameraChannel = CameraChannel.Value;
                setting.CameraName = CameraName.Value;

                if (SelectedCameraType.Value.Type == CameraType.NetworkCamera)
                {
                    // ネットワークカメラ用設定を保存する
                    var networkCameraSetting = new NetworkCameraSetting
                    {
                        UserName = _networkCameraSettingService.UserName.Value,
                        Password = _networkCameraSettingService.Password.Value,
                        IsLoggedIn = _networkCameraSettingService.IsLoggedIn.Value
                    };

                    if (_networkCameraSettingService.CameraDeviceSelectedItem.Value == null)
                        throw new ValidationException();
                    var deviceListItem = _networkCameraSettingService.CameraDeviceSelectedItem.Value;
                    networkCameraSetting.ProductName = deviceListItem.DeviceName.Value;
                    networkCameraSetting.IpAddress = deviceListItem.IpAddress.Value;

                    if (_networkCameraSettingService.SelectedNetworkCameraProfile.Value == null)
                        throw new ValidationException();
                    var profile = _networkCameraSettingService.SelectedNetworkCameraProfile.Value;
                    networkCameraSetting.ProfileToken = profile.ProfileToken;
                    networkCameraSetting.StreamUri = profile.StreamUri;
                    networkCameraSetting.Encoding = GetVideoEncodingName(profile.Encoding);
                    networkCameraSetting.CameraHeight = profile.VideoHeight;
                    networkCameraSetting.CameraWidth = profile.VideoWidth;

                    setting.NetworkCameraSettings = networkCameraSetting;
                }
                else if (SelectedCameraType.Value.Type == CameraType.UsbCamera)
                {
                    // USBカメラ用設定を保存する
                    var usbCameraSetting = new UsbCameraSetting
                    {
                        DevicePath = _usbCameraSettingService.CameraDeviceSelectedItem.Value.DevicePath,
                        CameraWidth = _usbCameraSettingService.SelectedUsbCameraVideoInfo.Value.Width,
                        CameraHeight = _usbCameraSettingService.SelectedUsbCameraVideoInfo.Value.Height,
                        FrameRate = _usbCameraSettingService.SelectedUsbCameraVideoInfo.Value.BitCount
                    };

                    setting.UsbCameraSettings = usbCameraSetting;
                }

                setting.RecordedFrameFolder = Path.GetFullPath(CameraSetting.GetRecordedFrameFolder(dbAccessor.GetRecordedFrameBaseFolder(), setting.CameraChannel));

                // DBにカメラ設定を保存する
                if (dbAccessor.UpdateOrInsertCameraSetting(setting))
                {
                    _networkCameraSettingService.StopCapture();
                    _usbCameraSettingService.StopCapture();

                    if (_mainWindowService.CaptureCameraClients.ContainsKey(setting.CameraChannel))
                    {
                        _mainWindowService.CaptureCameraClients[setting.CameraChannel].StopCapture();
                        _mainWindowService.CaptureCameraClients[setting.CameraChannel].Dispose();
                        _mainWindowService.CaptureCameraClients.Remove(setting.CameraChannel);
                    }

                    var newCameraClient = new CaptureCameraClient(setting);
                    newCameraClient.StartCapture();
                    _mainWindowService.CaptureCameraClients.Add(setting.CameraChannel, newCameraClient);
                }

                // 入力値をクリアする
                Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void OnCancelButtonClick()
        {
            // 入力値をクリアする
            Clear();

            // 1つ前の画面に戻る
            regionManager.RequestNavigate("ContentRegion", "CameraCommonPanel");
        }

        private void Clear()
        {
            CameraChannel.Value = 0;
            CameraName.Value = string.Empty;
            SelectedCameraType.Value = CameraTypeItems.FirstOrDefault();

            _networkCameraSettingService.Clear();
            _usbCameraSettingService.Clear();
        }
    }
}
