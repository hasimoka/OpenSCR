using HalationGhost.WinApps;
using OptionViews.Models;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.AdvancedCameraSettings
{
    class IpCameraSettingPanelViewModel : HalationGhostViewModelBase
    {
        private IContainerProvider container;

        public IpCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider) : base(regionMan)
        {
            this.container = containerProvider;

            this.IpCameraProfileItems = new ReactiveCollection<IpCameraProfile>()
                .AddTo(this.disposable);

            this.SelectedIpCameraProfile = new ReactiveProperty<IpCameraProfile>()
                .AddTo(this.disposable);

            this.ChangeSelectedIpCameraProfile = new ReactiveCommand()
                .WithSubscribe(() => this.onChangeSelectedIpCameraProfile())
                .AddTo(this.disposable);

            this.FrameRateLimit = new ReactiveProperty<int>()
                .AddTo(this.disposable);

            this.BitrateLimit = new ReactiveProperty<int>()
                .AddTo(this.disposable);
        }

        public ReactiveCollection<IpCameraProfile> IpCameraProfileItems { get; }

        public ReactiveProperty<IpCameraProfile> SelectedIpCameraProfile { get; }

        public ReactiveCommand ChangeSelectedIpCameraProfile { get; }

        public ReactiveProperty<int> FrameRateLimit { get; }

        public ReactiveProperty<int> BitrateLimit { get; }

        public void SetSetting(List<IpCameraProfile> profiles)
        {
            this.IpCameraProfileItems.Clear();

            if (profiles != null)
            {
                foreach (var profile in profiles.OrderBy(x => x.ProfileToken))
                {
                    this.IpCameraProfileItems.Add(profile);
                }

                var firstProfile = this.IpCameraProfileItems.FirstOrDefault();

                this.SelectedIpCameraProfile.Value = firstProfile;
                this.FrameRateLimit.Value = firstProfile.FrameRateLimit;
                this.BitrateLimit.Value = firstProfile.BitrateLimite;
            }
            else
            {
                this.SelectedIpCameraProfile.Value = null;
                this.FrameRateLimit.Value = 0;
                this.BitrateLimit.Value = 0;
            }
        }

        private void onChangeSelectedIpCameraProfile()
        {
            if (this.SelectedIpCameraProfile.Value != null)
            {
                this.FrameRateLimit.Value = this.SelectedIpCameraProfile.Value.FrameRateLimit;
                this.BitrateLimit.Value = this.SelectedIpCameraProfile.Value.BitrateLimite;
            }
        }
    }
}
