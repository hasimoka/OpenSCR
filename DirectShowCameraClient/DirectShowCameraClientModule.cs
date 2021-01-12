using DirectShowCameraClient.Models;
using Prism.Ioc;
using Prism.Modularity;

namespace DirectShowCameraClient
{
    public class DirectShowCameraClientModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IUsbCameraClient, UsbCameraClient>();
		}
	}
}
