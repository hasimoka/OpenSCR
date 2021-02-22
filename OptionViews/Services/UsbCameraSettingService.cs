using DirectShowCameraClient.Models;
using HalationGhost;
using OpenSCRLib;
using OptionViews.AdvancedCameraSettings;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OptionViews.Services
{
    /// <summary>
    ///  UsbCameraListPanelとUsbCameraSettingPanelの値を中継するサービスを表します
    /// </summary>
    public class UsbCameraSettingService : BindableModelBase, IUsbCameraSettingService
    {
        private readonly UsbCameraClient _cameraClient;

        public UsbCameraSettingService()
        {
            this._cameraClient = new UsbCameraClient();

            this.FrameImage = this._cameraClient.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.Disposable);

            this.CameraDeviceListSource = new ReactiveCollection<UsbCameraDeviceListItemViewModel>()
                .AddTo(this.Disposable);

            this.CameraDeviceSelectedItem = new ReactiveProperty<UsbCameraDeviceListItemViewModel>()
                .AddTo(this.Disposable);

            this.UsbCameraVideoInfoItems = new ReactiveCollection<UsbCameraVideoInfo>()
                .AddTo(this.Disposable);

            this.SelectedUsbCameraVideoInfo = new ReactiveProperty<UsbCameraVideoInfo>()
                .AddTo(this.Disposable);

            this.CanSelectionChangedCameraDeviceListCommand = new ReactivePropertySlim<bool>(true)
                .AddTo(this.Disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; set; }

        public ReactiveCollection<UsbCameraDeviceListItemViewModel> CameraDeviceListSource { get; set; }

        public ReactiveProperty<UsbCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveCollection<UsbCameraVideoInfo> UsbCameraVideoInfoItems { get; set; }

        public ReactiveProperty<UsbCameraVideoInfo> SelectedUsbCameraVideoInfo { get; set; }

        public ReactivePropertySlim<bool> CanSelectionChangedCameraDeviceListCommand { get; }

        public void Clear()
        {
            this._cameraClient.StopCapture();

            this._cameraClient.ClearFrameImage();

            this.CameraDeviceListSource.Clear();
            this.CameraDeviceSelectedItem.Value = null;

            this.UsbCameraVideoInfoItems.Clear();
            this.SelectedUsbCameraVideoInfo.Value = null;
        }

        public void FindCaptureDeviceAsync(Action<UsbCameraDeviceInfo> discoveriedAction)
        {
            this._cameraClient.FindUsbCaptureDeviceAsync(discoveriedAction);
        }

        public async Task RefreshCameraList()
        {
            var selectedUsbCameraVideoInfo = this.SelectedUsbCameraVideoInfo.Value;
            var slectedCaptureDeviceItem = this.CameraDeviceSelectedItem.Value;

            // 現在の設定をすべてクリアする
            this.Clear();

            try
            {
                var captureDevices = await this.DiscoveryCaptureDeviceAsync();
                foreach (var captureDevice in captureDevices.OrderBy(captureDevice => captureDevice.Name))
                {

                    var item = new UsbCameraDeviceListItemViewModel {CaptureDevice = captureDevice};

                    this.CameraDeviceListSource.Add(item);

                    if (slectedCaptureDeviceItem != null)
                    {
                        if (captureDevice.DevicePath == slectedCaptureDeviceItem.DevicePath)
                        {
                            this.CameraDeviceSelectedItem.Value = item;
                        }
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
                var videoInfos = await this.GetVideoInfosAsync(this.CameraDeviceSelectedItem.Value.CaptureDevice);

                foreach (var videoInfo in videoInfos.OrderByDescending(x => x.Width).ThenBy(x => x.Height).ThenBy(x => x.BitCount))
                {
                    this.UsbCameraVideoInfoItems.Add(videoInfo);

                    if (selectedUsbCameraVideoInfo != null)
                    {
                        if (videoInfo.Width == selectedUsbCameraVideoInfo.Width && videoInfo.Height == selectedUsbCameraVideoInfo.Height && videoInfo.BitCount == selectedUsbCameraVideoInfo.BitCount)
                        {
                            this.SelectedUsbCameraVideoInfo.Value = videoInfo;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task<List<UsbCameraDeviceInfo>> DiscoveryCaptureDeviceAsync()
        {
            return await this._cameraClient.DiscoveryCaptureDeviceAsync();
        }

        public async Task<List<UsbCameraVideoInfo>> GetVideoInfosAsync(UsbCameraDeviceInfo captureDevice)
        {
            return await this._cameraClient.GetVideoInfosAsync(captureDevice);
        }

        public void StartCapture()
        {
            var cameraSettings = new CameraSetting()
            {
                UsbCameraSettings = new UsbCameraSetting()
                {
                    DevicePath = CameraDeviceSelectedItem.Value.CaptureDevice.DevicePath,
                    CameraWidth = SelectedUsbCameraVideoInfo.Value.Width,
                    CameraHeight = SelectedUsbCameraVideoInfo.Value.Height,
                    FrameRate = SelectedUsbCameraVideoInfo.Value.BitCount
                }

            };

            this._cameraClient.StartCapture(cameraSettings);
        }

        public void StopCapture()
        {
            this._cameraClient.StopCapture();
        }
    }
}
