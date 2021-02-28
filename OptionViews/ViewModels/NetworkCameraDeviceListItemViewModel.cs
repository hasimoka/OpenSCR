using System;
using HalationGhost.WinApps;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace OptionViews.ViewModels
{
    public class NetworkCameraDeviceListItemViewModel : HalationGhostViewModelBase
    {
        public ReactiveProperty<string> DeviceName { get; }

        public ReactiveProperty<string> IpAddress { get; }

        public Uri EndpointUri { get; set; }

        public string Location { get; set; }

        public NetworkCameraDeviceListItemViewModel()
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
