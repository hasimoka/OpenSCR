using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.Models
{
    public interface ICommonCameraSetting
    {
        ReactivePropertySlim<string> CameraChannel { get; }

        ReactivePropertySlim<string> CameraName { get; }

        ReactivePropertySlim<string> Encoding { get; }

        ReactivePropertySlim<string> Resolution { get; }

        ReactiveCommand OpenCameraSettingButtonClick { get; }

        ReactiveCommand DeleteCameraSettingButtonClick { get; }
    }
}
