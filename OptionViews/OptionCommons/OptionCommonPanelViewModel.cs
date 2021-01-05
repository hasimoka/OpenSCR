using HalationGhost.WinApps;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OptionViews.OptionCommons
{
    class OptionCommonPanelViewModel : HalationGhostViewModelBase
    {
        /// <summary>
        /// 「カメラ設定ボタン」Clickコマンド
        /// </summary>
        public ReactiveCommand OpenCameraSettingClick { get; }

        public ReactiveCommand OpenRecordSettingClick { get; }

        public OptionCommonPanelViewModel(IRegionManager regionMan) : base(regionMan)
        {
            this.OpenCameraSettingClick = new ReactiveCommand()
                .WithSubscribe(() => this.onOpenCameraSettingClick())
                .AddTo(this.disposable);

            this.OpenRecordSettingClick = new ReactiveCommand()
                .WithSubscribe(() => this.onOpenRecordSettingClick())
                .AddTo(this.disposable);
        }

        private void onOpenCameraSettingClick()
        {
            Console.WriteLine("Call onOpenCameraSettingClick() method.");
            this.regionManager.RequestNavigate("ContentRegion", "CameraCommonPanel");
        }

        private void onOpenRecordSettingClick()
        {
            Console.WriteLine("Call onOpenRecordSettingClick() method.");
        }
    }
}
