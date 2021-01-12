using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mictlanix.DotNet.Onvif.Common;

namespace OpenSCRLib
{
    public class OnvifNetworkCameraProfile
    {
        public string ProfileToken { get; set; }

        public string StreamUri { get; set; }

        public VideoEncoding Encoding { get; set; }

        public int BitrateLimite { get; set; }

        public int EncodingInterval { get; set; }

        public int FrameRateLimit { get; set; }

        public int VideoHeight { get; set; }

        public int VideoWidth { get; set; }

        public bool CanPtzAbsoluteMove { get; set; }

        public bool CanPtzRelativeMove { get; set; }

        public bool CanPtzContinuousMove { get; set; }
    }
}
