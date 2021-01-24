using HalationGhost.WinApps;
using OpenSCRLib;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OptionViews.AdvancedCameraSettings
{
    class UsbCameraSettingPanelViewModel : HalationGhostViewModelBase
    {
        private IContainerProvider container;

        private IUsbCameraSettingService usbCameraSettingService;

        public UsbCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, IUsbCameraSettingService cameraSettingService) : base(regionMan)
        {
            this.container = containerProvider;
            this.usbCameraSettingService = cameraSettingService;

            this.FrameImage = this.usbCameraSettingService.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.disposable);

            this.UsbCameraVideoInfoItems = this.usbCameraSettingService.UsbCameraVideoInfoItems
                .AddTo(this.disposable);

            this.SelectedUsbCameraVideoInfo = this.usbCameraSettingService.SelectedUsbCameraVideoInfo
                .AddTo(this.disposable);

            this.ResolutionAndFrameRateItemSelectionChanged = new ReactiveCommand()
                .WithSubscribe(() => this.onResolutionAndFrameRateItemSelectionChanged())
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public ReactiveCollection<UsbCameraVideoInfo> UsbCameraVideoInfoItems { get; }

        public ReactiveProperty<UsbCameraVideoInfo> SelectedUsbCameraVideoInfo { get; }

        public ReactiveCommand ResolutionAndFrameRateItemSelectionChanged { get; }

        private void onResolutionAndFrameRateItemSelectionChanged()
        {
            if (this.SelectedUsbCameraVideoInfo.Value != null)
            {
                this.usbCameraSettingService.StartCapture();
            }
        }
    }
}
