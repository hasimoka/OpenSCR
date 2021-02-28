using HalationGhost;
using OptionViews.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace OptionViews.Services
{
    public class CommonCameraSettingService : BindableModelBase, ICommonCameraSettingService
    {
        public CommonCameraSettingService()
        {
            this.CameraSettings = new ReactiveCollection<CommonCameraSetting>()
                .AddTo(this.Disposable);
        }

        public ReactiveCollection<CommonCameraSetting> CameraSettings { get; }
    }
}
