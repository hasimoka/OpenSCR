using OpenSCRLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnvifNetworkCameraClient.Models
{
    public interface IWsDiscoveryClient : IDisposable
    {
        Task<List<NetworkCameraEndpoint>> DiscoveryNetworkVideoTransmitterAsync();
    }
}
