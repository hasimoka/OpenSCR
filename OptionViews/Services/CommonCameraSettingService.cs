using HalationGhost;
using OptionViews.CammeraCommons;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
