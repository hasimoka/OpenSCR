using OptionViews.Services;
using OptionViews.ViewModels;
using OptionViews.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace OptionViews
{
    public class OptionViewsModule : IModule
	{
		public void OnInitialized(IContainerProvider containerProvider)
		{

		}

		public void RegisterTypes(IContainerRegistry containerRegistry)
		{
			containerRegistry.RegisterSingleton<ICommonCameraSettingService, CommonCameraSettingService>();
			containerRegistry.RegisterSingleton<INetworkCameraSettingService, NetworkCameraSettingService>();
			containerRegistry.RegisterSingleton<IUsbCameraSettingService, UsbCameraSettingService>();

			containerRegistry.RegisterForNavigation<OptionCommonPanel, OptionCommonPanelViewModel>();
			containerRegistry.RegisterForNavigation<CameraCommonPanel, CameraCommonPanelViewModel>();
			containerRegistry.RegisterForNavigation<AdvancedCameraSettingPanel, AdvancedCameraSettingPanelViewModel>();
			containerRegistry.RegisterForNavigation<IpCameraListPanel, NetworkCameraListPanelViewModel>();
			containerRegistry.RegisterForNavigation<CameraLoginSettingPanel, CameraLoginSettingPanelViewModel>();
			containerRegistry.RegisterForNavigation<CameraLogoutSettingPanel, CameraLogoutSettingPanelViewModel>();
			containerRegistry.RegisterForNavigation<UsbCameraListPanel, UsbCameraListPanelViewModel>();
			containerRegistry.RegisterForNavigation<IpCameraSettingPanel, NetworkCameraSettingPanelViewModel>();
			containerRegistry.RegisterForNavigation<UsbCameraSettingPanel, UsbCameraSettingPanelViewModel>();
		}
	}
}
