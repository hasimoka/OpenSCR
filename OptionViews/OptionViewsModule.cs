using OptionViews.AdvancedCameraSettings;
using OptionViews.CameraLoginSettings;
using OptionViews.CameraLogoutSettings;
using OptionViews.CammeraCommons;
using OptionViews.Models;
using OptionViews.OptionCommons;
using OptionViews.Services;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
