using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Reactive.Bindings;

namespace OpenSCRLib
{
    public interface ICameraClient : IDisposable
    {
        ReactivePropertySlim<BitmapSource> FrameImage { get; }

        void StartCapture(CameraSetting cameraSettings);

        void StopCapture();

        void ClearFrameImage();
    }
}
