using HalationGhost.WinApps;
using OpenSCRLib;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using MainWindowServices;

namespace OptionViews.Models
{
    public class CommonCameraSetting : HalationGhostViewModelBase, ICommonCameraSetting
    {
        private readonly IContainerProvider _container;

        private readonly ICommonCameraSettingService _commonCameraSettingService;

        private readonly IMainWindowService _mainWindowService;

        private readonly CameraSetting _cameraSetting;

        public CommonCameraSetting(CameraSetting cameraSetting, IRegionManager regionMan, IContainerProvider container, IMainWindowService windowService, ICommonCameraSettingService commonCameraSettingService) : base(regionMan)
        {
            _cameraSetting = cameraSetting;

            _container = container;
            _mainWindowService = windowService;
            _commonCameraSettingService = commonCameraSettingService;

            
            CameraChannel = new ReactivePropertySlim<string>($"{cameraSetting.CameraChannel:00} CH")
                .AddTo(this.disposable);

            CameraName = new ReactivePropertySlim<string>(cameraSetting.CameraName)
                .AddTo(this.disposable);

            var encodingValue = string.Empty;
            if (cameraSetting.NetworkCameraSettings != null)
            {
                // "EncodingFormat(IP Address)"のフォーマット
                encodingValue = $"{cameraSetting.NetworkCameraSettings.Encoding} ({cameraSetting.NetworkCameraSettings.IpAddress})";
            }
            else if (cameraSetting.UsbCameraSettings != null)
            {
                encodingValue = "USB";
            }
            Encoding = new ReactivePropertySlim<string>(encodingValue)
                .AddTo(disposable);

            var resolutionValue = string.Empty;
            if (cameraSetting.NetworkCameraSettings != null)
            {
                resolutionValue = $"{cameraSetting.NetworkCameraSettings.CameraWidth} x {cameraSetting.NetworkCameraSettings.CameraHeight}";
            }
            else if (cameraSetting.UsbCameraSettings != null)
            {
                resolutionValue = $"{cameraSetting.UsbCameraSettings.CameraWidth} x {cameraSetting.UsbCameraSettings.CameraHeight}";
            }
            Resolution = new ReactivePropertySlim<string>(resolutionValue)
                .AddTo(disposable);


            OpenCameraSettingButtonClick = new ReactiveCommand()
                .WithSubscribe(() => OnOpenCameraSettingButtonClick())
                .AddTo(disposable);

            DeleteCameraSettingButtonClick = new ReactiveCommand()
                .WithSubscribe(() => OnDeleteCameraSettingButtonClick())
                .AddTo(disposable);
        }

        public ReactivePropertySlim<string> CameraChannel { get; }

        public ReactivePropertySlim<string> CameraName { get; }

        public ReactivePropertySlim<string> Encoding { get; }

        public ReactivePropertySlim<string> Resolution { get; }

        public ReactiveCommand OpenCameraSettingButtonClick { get; }

        public ReactiveCommand DeleteCameraSettingButtonClick { get; }

        private void OnOpenCameraSettingButtonClick()
        {
            Console.WriteLine("Call OnOpenCameraSettingButtonClick() method.");

            var parameter = new NavigationParameters();
            parameter.Add("CameraSetting", _cameraSetting);
            this.regionManager.RequestNavigate("ContentRegion", "AdvancedCameraSettingPanel", parameter);
        }

        private void OnDeleteCameraSettingButtonClick()
        {
            Console.WriteLine("Call OnDeleteCameraSettingButtonClick() method.");
            var dbAccessor = this._container.Resolve<DatabaseAccessor>();
            if (dbAccessor.DeleteCameraSetting(_cameraSetting))
            {
                if (_mainWindowService.CaptureCameraClients.ContainsKey(_cameraSetting.CameraChannel))
                {
                    _mainWindowService.CaptureCameraClients[_cameraSetting.CameraChannel].StopCapture();
                    _mainWindowService.CaptureCameraClients[_cameraSetting.CameraChannel].Dispose();
                    _mainWindowService.CaptureCameraClients.Remove(_cameraSetting.CameraChannel);
                }
            }

            this._commonCameraSettingService.CameraSettings.Remove(this);
        }
    }
}
