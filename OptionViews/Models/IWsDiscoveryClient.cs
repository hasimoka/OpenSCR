﻿using OpenSCRLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.Models
{
    public interface IWsDiscoveryClient : IDisposable
    {
        void FindNetworkVideoTransmitterAsync(Action<IpCameraDeviceInfo> deviceDiscoveriedAction);
    }
}