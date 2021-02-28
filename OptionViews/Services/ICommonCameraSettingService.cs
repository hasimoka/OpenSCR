using OptionViews.Models;
using Reactive.Bindings;

namespace OptionViews.Services
{
    public interface ICommonCameraSettingService
    {
        ReactiveCollection<CommonCameraSetting> CameraSettings { get; }
    }
}
