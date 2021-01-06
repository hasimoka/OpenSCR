using HalationGhost.WinApps;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.AdvancedCameraSettings
{
    public class UsbCameraDeviceListItemViewModel : HalationGhostViewModelBase
    {
        public ReactiveProperty<string> DeviceName { get; }

        public UsbCameraDeviceListItemViewModel()
        {
            this.DeviceName = new ReactiveProperty<string>("")
                .AddTo(this.disposable);
        }
    }
}
