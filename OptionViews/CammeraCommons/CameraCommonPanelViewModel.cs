using HalationGhost.WinApps;
using OptionViews.AdvancedCameraSettings;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Prism.Ioc;

namespace OptionViews.CammeraCommons
{
    class CameraCommonPanelViewModel : HalationGhostViewModelBase
    {
        public ReactiveCommand ReturnOptionCommonPanelClick { get; }

        public ReactiveCommand AddCameraSettingClick { get; }

        public CameraCommonPanelViewModel(IRegionManager regionMan) : base(regionMan)
        {
            this.ReturnOptionCommonPanelClick = new ReactiveCommand()
                .WithSubscribe(() => this.onReturnOptionCommonPanelClick())
                .AddTo(this.disposable);

            this.AddCameraSettingClick = new ReactiveCommand()
                .WithSubscribe(() => this.onAddCameraSettingClick())
                .AddTo(this.disposable);
        }

        private void onReturnOptionCommonPanelClick()
        {
            Console.WriteLine("Call onReturnOptionCommonPanelClick() method.");
            this.regionManager.RequestNavigate("ContentRegion", "OptionCommonPanel");
        }

        private void onAddCameraSettingClick()
        {
            Console.WriteLine("Call onAddCameraSettingClick() method.");
            this.regionManager.RequestNavigate("ContentRegion", "AdvancedCameraSettingPanel");
        }
    }
}
