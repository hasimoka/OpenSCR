using OptionViews.AdvancedCameraSettings;
using OptionViews.CameraLoginSettings;
using OptionViews.CameraLogoutSettings;
using OptionViews.CammeraCommons;
using OptionViews.OptionCommons;
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
			containerRegistry.RegisterForNavigation<OptionCommonPanel, OptionCommonPanelViewModel>();
			containerRegistry.RegisterForNavigation<CameraCommonPanel, CameraCommonPanelViewModel>();
			containerRegistry.RegisterForNavigation<AdvancedCameraSettingPanel, AdvancedCameraSettingPanelViewModel>();
			containerRegistry.RegisterForNavigation<CameraLoginSettingPanel, CameraLoginSettingPanelViewModel>();
			containerRegistry.RegisterForNavigation<CameraLogoutSettingPanel, CameraLogoutSettingPanelViewModel>();
		}
	}
}
