using HalationGhost.WinApps;
using MainWindowServices;
using OpenSCRLib;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OptionViews.AdvancedCameraSettings
{
    class UsbCameraListPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        private IMainWindowService mainWindowService;

        private IUsbCameraSettingService usbCameraSettingService;

        public UsbCameraListPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService, IUsbCameraSettingService cameraSettingService) : base(regionMan)
        {
            this.container = containerProvider;
            this.mainWindowService = windowService;
            this.usbCameraSettingService = cameraSettingService;

            this.CameraDeviceListSource = this.usbCameraSettingService.CameraDeviceListSource
                .AddTo(this.disposable);

            this.CameraDeviceSelectedItem = this.usbCameraSettingService.CameraDeviceSelectedItem
                .AddTo(this.disposable);

            this.IsProgressRingDialogOpen = this.mainWindowService.IsProgressRingDialogOpen
                .AddTo(this.disposable);

            this.CanSelectionChangedCameraDeviceListCommand = new ReactiveProperty<bool>(true)
                .AddTo(this.disposable);

            this.SelectionChangedCameraDeviceListCommand = this.CanSelectionChangedCameraDeviceListCommand
                .ToAsyncReactiveCommand<SelectionChangedEventArgs>()
                .WithSubscribe(async (item) =>
                {
                    // ProgressRingダイアログを表示にする
                    this.IsProgressRingDialogOpen.Value = true;

                    await this.onSelectionChangedCameraDeviceListAsync(item);

                    // ProgressRingダイアログを非表示にする
                    this.IsProgressRingDialogOpen.Value = false;
                })
                .AddTo(this.disposable);

            this.SetInitializeParameterCommand = this.CanSelectionChangedCameraDeviceListCommand
                .ToAsyncReactiveCommand<UsbCameraSetting>()
                .WithSubscribe(async (usbCameraSetting) =>
                {
                    // ProgressRingダイアログを表示にする
                    this.IsProgressRingDialogOpen.Value = true;

                    await this.onSetInitializeParameterCommand(usbCameraSetting);

                    // ProgressRingダイアログを非表示にする
                    this.IsProgressRingDialogOpen.Value = false;
                })
                .AddTo(this.disposable);
        }

        public ReactiveCollection<UsbCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<UsbCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveProperty<bool> CanSelectionChangedCameraDeviceListCommand { get; }

        public AsyncReactiveCommand<SelectionChangedEventArgs> SelectionChangedCameraDeviceListCommand { get; }

        public AsyncReactiveCommand<UsbCameraSetting> SetInitializeParameterCommand { get; }

        public ReactivePropertySlim<bool> IsProgressRingDialogOpen { get; set; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var usbCameraSetting = navigationContext.Parameters["UsbCameraSettings"] as UsbCameraSetting;
            this.SetInitializeParameterCommand.Execute(usbCameraSetting);
        }

        private async Task onSelectionChangedCameraDeviceListAsync(SelectionChangedEventArgs args)
        {
            Console.WriteLine($"Call onSelectionChangedCameraDeviceList() method. [args:{args}]");

            if (this.CameraDeviceSelectedItem.Value != null)
            {
                try
                {
                    // 接続するカメラの解像度・ビットレートを取得する
                    var captureDevice = this.CameraDeviceSelectedItem.Value.CaptureDevice;
                    var videoInfos = await this.usbCameraSettingService.GetVideoInfosAsync(captureDevice);

                    // UsbCameraSettingPanelに取得したカメラ解像度・ビットレートを反映させる
                    this.usbCameraSettingService.UsbCameraVideoInfoItems.Clear();
                    foreach (var videoInfo in videoInfos.OrderByDescending(x => x.Width).ThenBy(x => x.Height).ThenBy(x => x.BitCount))
                    {
                        this.usbCameraSettingService.UsbCameraVideoInfoItems.Add(videoInfo);
                    }
                    this.usbCameraSettingService.SelectedUsbCameraVideoInfo.Value = this.usbCameraSettingService.UsbCameraVideoInfoItems.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                this.usbCameraSettingService.UsbCameraVideoInfoItems.Clear();
                this.usbCameraSettingService.SelectedUsbCameraVideoInfo.Value = null;
            }
        }

        private async Task onSetInitializeParameterCommand(UsbCameraSetting usbCameraSetting)
        {
            // 現在の設定をすべてクリアする
            this.usbCameraSettingService.Clear();

            if (usbCameraSetting != null)
            {
                try
                {
                    var captureDevices = await this.usbCameraSettingService.DiscoveryCaptureDeviceAsync();
                    foreach (var captureDevice in captureDevices.OrderBy(captureDevice => captureDevice.Name))
                    {

                        var item = this.container.Resolve<UsbCameraDeviceListItemViewModel>();
                        item.CaptureDevice = captureDevice;

                        this.CameraDeviceListSource.Add(item);

                        if (captureDevice.DevicePath == usbCameraSetting.DevicePath)
                        {
                            this.CameraDeviceSelectedItem.Value = item;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                // 制御を一旦手放して、SelectionChangedEventを発火させる
                // ※イベントは発火するが、ReactiveCommandのCanExecuteがfalseになっているのでコマンドは実行されない
                await Task.Delay(TimeSpan.FromMilliseconds(100));

                try
                {
                    // 接続するカメラのビデオ情報を取得する
                    var videoInfos = await this.usbCameraSettingService.GetVideoInfosAsync(this.CameraDeviceSelectedItem.Value.CaptureDevice);

                    foreach (var videoInfo in videoInfos.OrderByDescending(x => x.Width).ThenBy(x => x.Height).ThenBy(x => x.BitCount))
                    {
                        this.usbCameraSettingService.UsbCameraVideoInfoItems.Add(videoInfo);

                        if (videoInfo.Width == usbCameraSetting.CameraWidth && videoInfo.Height == usbCameraSetting.CameraHeight && videoInfo.BitCount == usbCameraSetting.FrameRate)
                        {
                            this.usbCameraSettingService.SelectedUsbCameraVideoInfo.Value = videoInfo;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            else
            {
                this.regionManager.RequestNavigate("CameraLoginSettingRegion", "CameraLoginSettingPanel");

                try
                {
                    var captureDevices = await this.usbCameraSettingService.DiscoveryCaptureDeviceAsync();
                    foreach (var captureDevice in captureDevices.OrderBy(captureDevice => captureDevice.Name))
                    {

                        var item = this.container.Resolve<UsbCameraDeviceListItemViewModel>();
                        item.CaptureDevice = captureDevice;

                        this.CameraDeviceListSource.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
