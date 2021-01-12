using HalationGhost.WinApps;
using OpenSCRLib;
using OptionViews.Models;
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
using System.Windows.Media.Imaging;

namespace OptionViews.AdvancedCameraSettings
{
    class NetworkCameraSettingPanelViewModel : HalationGhostViewModelBase
    {
        private IContainerProvider container;

        private INetworkCameraSettingService networkCameraSettingService;

        public NetworkCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider, INetworkCameraSettingService cameraSettingService) : base(regionMan)
        {
            this.container = containerProvider;
            this.networkCameraSettingService = cameraSettingService;

            this.FrameImage = this.networkCameraSettingService.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(this.disposable);

            this.NetworkCameraProfileItems = this.networkCameraSettingService.NetworkCameraProfileItems
                .AddTo(this.disposable);

            this.SelectedNetworkCameraProfile = this.networkCameraSettingService.SelectedNetworkCameraProfile
                .AddTo(this.disposable);

            this.NetworkCameraProfileItemSelectionChanged = new ReactiveCommand()
                .WithSubscribe(() => this.onNetworkCameraProfileItemSelectionChanged())
                .AddTo(this.disposable);

            this.FrameRateLimit = this.networkCameraSettingService.FrameRateLimit
                .AddTo(this.disposable);

            this.BitrateLimit = this.networkCameraSettingService.BitrateLimit
                .AddTo(this.disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public ReactiveCollection<OnvifNetworkCameraProfile> NetworkCameraProfileItems { get; }

        public ReactiveProperty<OnvifNetworkCameraProfile> SelectedNetworkCameraProfile { get; }

        public ReactiveCommand NetworkCameraProfileItemSelectionChanged { get; }

        public ReactiveProperty<int> FrameRateLimit { get; }

        public ReactiveProperty<int> BitrateLimit { get; }

        public void SetSetting(List<OnvifNetworkCameraProfile> profiles)
        {
            this.NetworkCameraProfileItems.Clear();

            if (profiles != null)
            {
                foreach (var profile in profiles.OrderBy(x => x.ProfileToken))
                {
                    this.NetworkCameraProfileItems.Add(profile);
                }

                var firstProfile = this.NetworkCameraProfileItems.FirstOrDefault();

                this.SelectedNetworkCameraProfile.Value = firstProfile;
                this.FrameRateLimit.Value = firstProfile.FrameRateLimit;
                this.BitrateLimit.Value = firstProfile.BitrateLimite;
            }
            else
            {
                this.SelectedNetworkCameraProfile.Value = null;
                this.FrameRateLimit.Value = 0;
                this.BitrateLimit.Value = 0;
            }
        }

        private void onNetworkCameraProfileItemSelectionChanged()
        {
            if (this.SelectedNetworkCameraProfile.Value != null)
            {
                this.FrameRateLimit.Value = this.SelectedNetworkCameraProfile.Value.FrameRateLimit;
                this.BitrateLimit.Value = this.SelectedNetworkCameraProfile.Value.BitrateLimite;

                this.networkCameraSettingService.StartCapture();
            }
        }
    }
}
