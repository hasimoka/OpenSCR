using HalationGhost.WinApps;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionViews.AdvancedCameraSettings
{
    class IpCameraSettingPanelViewModel : HalationGhostViewModelBase
    {
        private IContainerProvider container;

        public IpCameraSettingPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider) : base(regionMan)
        {
            this.container = containerProvider;
        }
    }
}
