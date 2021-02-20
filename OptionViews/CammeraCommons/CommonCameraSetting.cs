using HalationGhost;
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

namespace OptionViews.CammeraCommons
{
    public class CommonCameraSetting : HalationGhostViewModelBase, ICommonCameraSetting
    {
        private IContainerProvider container;

        private ICommonCameraSettingService commonCameraSettingService;

        private CameraSetting cameraSetting;

        public CommonCameraSetting(CameraSetting cameraSetting, IRegionManager regionMan, IContainerProvider container, ICommonCameraSettingService commonCameraSettingService) : base(regionMan)
        {
            this.container = container;
            this.commonCameraSettingService = commonCameraSettingService;

            this.cameraSetting = cameraSetting;

            this.CameraChannel = new ReactivePropertySlim<string>($"{cameraSetting.CameraChannel:00} CH")
                .AddTo(this.disposable);

            this.CameraName = new ReactivePropertySlim<string>(cameraSetting.CameraName)
                .AddTo(this.disposable);

            var encodingValue = string.Empty;
            if (cameraSetting.NetworkCameraSetting != null)
            {
                // "EncodingFormat(IP Address)"のフォーマット
                encodingValue = $"{cameraSetting.NetworkCameraSetting.Encoding} ({cameraSetting.NetworkCameraSetting.IpAddress})";
            }
            else if (cameraSetting.UsbCameraSetting != null)
            {
                encodingValue = "USB";
            }
            this.Encoding = new ReactivePropertySlim<string>(encodingValue)
                .AddTo(this.disposable);

            var resoluutionValue = string.Empty;
            if (cameraSetting.NetworkCameraSetting != null)
            {
                resoluutionValue = $"{cameraSetting.NetworkCameraSetting.CameraWidth} x {cameraSetting.NetworkCameraSetting.CameraHeight}";
            }
            else if (cameraSetting.UsbCameraSetting != null)
            {
                resoluutionValue = $"{cameraSetting.UsbCameraSetting.CameraWidth} x {cameraSetting.UsbCameraSetting.CameraHeight}";
            }
            this.Resolution = new ReactivePropertySlim<string>(resoluutionValue)
                .AddTo(this.disposable);


            this.OpenCameraSettingButtonClick = new ReactiveCommand()
                .WithSubscribe(() => this.onOpenCameraSettingButtonClick())
                .AddTo(this.disposable);

            this.DeleteCameraSettingButtonClick = new ReactiveCommand()
                .WithSubscribe(() => this.onDeleteCameraSettingButtonClick())
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<string> CameraChannel { get; }

        public ReactivePropertySlim<string> CameraName { get; }

        public ReactivePropertySlim<string> Encoding { get; }

        public ReactivePropertySlim<string> Resolution { get; }

        public ReactiveCommand OpenCameraSettingButtonClick { get; }

        public ReactiveCommand DeleteCameraSettingButtonClick { get; }

        private void onOpenCameraSettingButtonClick()
        {
            Console.WriteLine("Call onOpenCameraSettingButtonClick() method.");

            var parameter = new NavigationParameters();
            parameter.Add("CameraSetting", this.cameraSetting);
            this.regionManager.RequestNavigate("ContentRegion", "AdvancedCameraSettingPanel", parameter);
        }

        private void onDeleteCameraSettingButtonClick()
        {
            Console.WriteLine("Call onDeleteCameraSettingButtonClick() method.");
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            dbAccessor.DeleteCameraSetting(this.cameraSetting);

            this.commonCameraSettingService.CameraSettings.Remove(this);
        }
    }
}
