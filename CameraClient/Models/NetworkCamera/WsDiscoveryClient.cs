using System;
using System.Collections.Generic;
using System.ServiceModel.Discovery;
using System.Threading.Tasks;
using System.Xml;
using OpenSCRLib;

namespace CameraClient.Models.NetworkCamera
{
    public class WsDiscoveryClient : IWsDiscoveryClient
    {
        private DiscoveryClient discoveryClient;

        public WsDiscoveryClient()
        {

            var endPoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
            this.discoveryClient = new DiscoveryClient(endPoint);
        }

        public void Dispose() { }

        public async Task<List<NetworkCameraEndpoint>> DiscoveryNetworkVideoTransmitterAsync()
        {
            return await Task.Run(() => 
            {
                var results = new List<NetworkCameraEndpoint>();

                try
                {
                    var endPoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
                    this.discoveryClient = new DiscoveryClient(endPoint);

                    FindCriteria findCriteria = new FindCriteria();
                    // タイムアウト時間を2秒に設定
                    findCriteria.Duration = new TimeSpan(0, 0, 2);
                    findCriteria.MaxResults = int.MaxValue;
                    // Edit: optionally specify contract type, ONVIF v1.0
                    findCriteria.ContractTypeNames.Add(
                        new XmlQualifiedName(
                            "NetworkVideoTransmitter",
                            "http://www.onvif.org/ver10/network/wsdl"));

                    var foundDevices = this.discoveryClient.Find(findCriteria);
                    foreach (var endpoint in foundDevices.Endpoints)
                    {
                        foreach (var uri in endpoint.ListenUris)
                        {
                            if (uri.HostNameType == UriHostNameType.IPv4)
                            {
                                try
                                {
                                    var networkCameraEndpoint = new NetworkCameraEndpoint();
                                    networkCameraEndpoint.EndpointUri = uri;

                                    foreach (var scope in endpoint.Scopes)
                                    {
                                        if (scope.Segments[1].Replace("/", string.Empty).ToLower() == "name")
                                        {
                                            if (scope.Segments.Length > 2)
                                            {
                                                networkCameraEndpoint.DeviceName = scope.Segments[2].Replace("/", string.Empty);
                                            }
                                        }
                                        else if (scope.Segments[1].Replace("/", string.Empty).ToLower() == "location")
                                        {
                                            if (scope.Segments.Length > 2)
                                            {
                                                networkCameraEndpoint.Location = scope.Segments[2].Replace("/", string.Empty);
                                            }
                                        }
                                    }

                                    results.Add(networkCameraEndpoint);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return results;
            });
        }
    }
}
