using DirectShowUsbCameraClient;
using HalationGhost;
using OpenSCRLib;
using OptionViews.AdvancedCameraSettings;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OptionViews.Services
{
    /// <summary>
    ///  UsbCameraListPanelとUsbCameraSettingPanelの値を中継するサービスを表します
    /// </summary>
    public class UsbCameraSettingService : BindableModelBase, IUsbCameraSettingService
    {
        private UsbCameraClient client;

        public UsbCameraSettingService()
        {
            this.client = new UsbCameraClient();

            this.FrameImage = this.client.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.Disposable);

            this.CameraDeviceListSource = new ReactiveCollection<UsbCameraDeviceListItemViewModel>()
                .AddTo(this.Disposable);

            this.CameraDeviceSelectedItem = new ReactiveProperty<UsbCameraDeviceListItemViewModel>()
                .AddTo(this.Disposable);

            this.SelectedUsbCameraDeviceInfo = new ReactiveProperty<UsbCameraDeviceInfo>()
                .AddTo(this.Disposable);

            this.UsbCameraVideoInfoItems = new ReactiveCollection<UsbCameraVideoInfo>()
                .AddTo(this.Disposable);

            this.SelectedUsbCameraVideoInfo = new ReactiveProperty<UsbCameraVideoInfo>()
                .AddTo(this.Disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; set; }

        public ReactiveCollection<UsbCameraDeviceListItemViewModel> CameraDeviceListSource { get; set; }

        public ReactiveProperty<UsbCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveProperty<UsbCameraDeviceInfo> SelectedUsbCameraDeviceInfo { get; set; }

        public ReactiveCollection<UsbCameraVideoInfo> UsbCameraVideoInfoItems { get; set; }

        public ReactiveProperty<UsbCameraVideoInfo> SelectedUsbCameraVideoInfo { get; set; }

        public void FindCaptureDeviceAsync(Action<UsbCameraDeviceInfo> discoveriedAction)
        {
            this.client.FindUsbCaptureDeviceAsync(discoveriedAction);
        }

        public List<UsbCameraVideoInfo> GetVideoInfosAsync(UsbCameraDeviceInfo captureDevice)
        {
            return this.client.GetVideoInfosAsync(captureDevice);
        }

        public void StartCapture()
        {
            this.client.StartCapture(
                this.CameraDeviceSelectedItem.Value.CaptureDevice.DevicePath, 
                this.SelectedUsbCameraVideoInfo.Value.Width,
                this.SelectedUsbCameraVideoInfo.Value.Height,
                this.SelectedUsbCameraVideoInfo.Value.BitCount); ;
        }
    }
}
