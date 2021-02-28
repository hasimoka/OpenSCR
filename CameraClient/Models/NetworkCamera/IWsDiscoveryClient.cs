using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenSCRLib;

namespace CameraClient.Models.NetworkCamera
{
    public interface IWsDiscoveryClient : IDisposable
    {
        Task<List<NetworkCameraEndpoint>> DiscoveryNetworkVideoTransmitterAsync();
    }
}
