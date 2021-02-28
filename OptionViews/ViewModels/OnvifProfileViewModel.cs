using HalationGhost.WinApps;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace OptionViews.ViewModels
{
    public class OnvifProfileViewModel : HalationGhostViewModelBase
    {
        public ReactiveProperty<string> ProfileName { get; }

        public ReactiveProperty<string> StreamUri { get; }

        public ReactiveProperty<string> Encoder { get; }

        public ReactiveProperty<int> ResolutionWidth { get; }

        public ReactiveProperty<int> ResolutionHeight { get; }

        public ReactiveProperty<int> FrameRateLimit { get; }

        public ReactiveProperty<int> BitrateLimit { get; }

        public OnvifProfileViewModel()
        {
            this.ProfileName = new ReactiveProperty<string>()
                .AddTo(this.disposable);

            this.StreamUri = new ReactiveProperty<string>()
                .AddTo(this.disposable);

            this.Encoder = new ReactiveProperty<string>()
                .AddTo(this.disposable);

            this.ResolutionWidth = new ReactiveProperty<int>(0)
                .AddTo(this.disposable);

            this.ResolutionHeight = new ReactiveProperty<int>(0)
                .AddTo(this.disposable);

            this.FrameRateLimit = new ReactiveProperty<int>(0)
                .AddTo(this.disposable);

            this.BitrateLimit = new ReactiveProperty<int>(0)
                .AddTo(this.disposable);
        }
    }
}
