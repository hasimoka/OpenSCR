﻿using System;
using HalationGhost.WinApps;
using OpenSCRLib;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace OptionViews.ViewModels
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

        public string DevicePath { get { return  this._captureDevice != null ? this._captureDevice.DevicePath : string.Empty; } }

        public ReactiveProperty<string> DeviceName { get; }
    }
}