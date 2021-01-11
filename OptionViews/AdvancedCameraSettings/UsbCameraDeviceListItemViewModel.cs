using HalationGhost.WinApps;
using OpenSCRLib;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.AdvancedCameraSettings
{
    public class UsbCameraDeviceListItemViewModel : HalationGhostViewModelBase
    {
        private UsbCameraDeviceInfo _captureDevice;

        public UsbCameraDeviceListItemViewModel()
        {
            this.DeviceName = new ReactiveProperty<string>(String.Empty)
                .AddTo(this.disposable);
        }

        public UsbCameraDeviceInfo CaptureDevice
        {
            get
            {
                return this._captureDevice;
            }

            set
            {
                this._captureDevice = value;

                this.DeviceName.Value = this._captureDevice.Name;
            }
        }

        public ReactiveProperty<string> DeviceName { get; }

    }
}
