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

        private Action<UsbCameraDeviceInfo> cameraDiscoveryedAction;

        public UsbCameraListPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IMainWindowService windowService, IUsbCameraSettingService cameraSettingService) : base(regionMan)
        {
            this.container = containerProvider;
            this.mainWindowService = windowService;
            this.usbCameraSettingService = cameraSettingService;

            this.CameraDeviceListSource = this.usbCameraSettingService.CameraDeviceListSource
                .AddTo(this.disposable);

            this.CameraDeviceSelectedItem = this.usbCameraSettingService.CameraDeviceSelectedItem
                .AddTo(this.disposable);

            this.SelectionChangedCameraDeviceList = new ReactiveCommand<SelectionChangedEventArgs>()
                .WithSubscribe((item) => 
                {
                    // ProgressRingダイアログを表示にする(onSelectionChangedCameraDeviceListAsync()終了時に非表示にする)
                    this.IsProgressRingDialogOpen.Value = true;
                    this.onSelectionChangedCameraDeviceListAsync(item);
                })
                .AddTo(this.disposable);

            this.IsProgressRingDialogOpen = this.mainWindowService.IsProgressRingDialogOpen
                .AddTo(this.disposable);

            this.cameraDiscoveryedAction = (cameraDeviceInfo) =>
            {
                var item = this.container.Resolve<UsbCameraDeviceListItemViewModel>();
                item.CaptureDevice = cameraDeviceInfo;

                this.CameraDeviceListSource.Add(item);
            };
            this.usbCameraSettingService.FindCaptureDeviceAsync(cameraDiscoveryedAction);
        }

        public ReactiveCollection<UsbCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<UsbCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveCommand<SelectionChangedEventArgs> SelectionChangedCameraDeviceList { get; }

        public ReactivePropertySlim<bool> IsProgressRingDialogOpen { get; set; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //this.RefreshCameraList();
        }

        public void RefreshCameraList()
        {
            this.CameraDeviceListSource.Clear();
            this.usbCameraSettingService.FindCaptureDeviceAsync(this.cameraDiscoveryedAction);
        }

        private async void onSelectionChangedCameraDeviceListAsync(SelectionChangedEventArgs args)
        {
            Console.WriteLine($"Call onSelectionChangedCameraDeviceList() method. [args:{args}]");

            if (this.CameraDeviceSelectedItem.Value != null)
            {
                try
                {
                    // 接続するカメラの解像度・ビットレートを取得する
                    var captureDevice = this.CameraDeviceSelectedItem.Value.CaptureDevice;
                    var videoInfos = await Task.Run(() =>
                    {
                       return this.usbCameraSettingService.GetVideoInfosAsync(captureDevice);
                    });

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

            // ProgressRingダイアログを非表示にする
            this.IsProgressRingDialogOpen.Value = false;
        }
    }
}
