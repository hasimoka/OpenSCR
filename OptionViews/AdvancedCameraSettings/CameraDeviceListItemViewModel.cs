﻿using HalationGhost.WinApps;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;

namespace OptionViews.AdvancedCameraSettings
{
    class CameraDeviceListItemViewModel : HalationGhostViewModelBase
    {
        public ReactiveProperty<string> DeviceName { get; }

        public ReactiveProperty<string> IpAddress { get; }

        public Uri EndpointUri { get; set; }

        public string Location { get; set; }

        public CameraDeviceListItemViewModel()
        {
            this.DeviceName = new ReactiveProperty<string>("")
                .AddTo(this.disposable);

            this.IpAddress = new ReactiveProperty<string>("")
                .AddTo(this.disposable);

            this.EndpointUri = null;

            this.Location = string.Empty;
        }
    }
}
