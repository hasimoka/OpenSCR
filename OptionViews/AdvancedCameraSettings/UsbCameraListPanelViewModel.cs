using HalationGhost.WinApps;
using OpenSCRLib;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OptionViews.AdvancedCameraSettings
{
    class UsbCameraListPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        public UsbCameraListPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider) : base(regionMan)
        {
            this.container = containerProvider;

            this.CameraDeviceListSource = new ReactiveCollection<UsbCameraDeviceListItemViewModel>()
                .AddTo(this.disposable);

            this.CameraDeviceSelectedItem = new ReactiveProperty<UsbCameraDeviceListItemViewModel>()
                .AddTo(this.disposable);

            Action<UsbCameraDeviceInfo> cameraDiscoveryedAction = (cameraDeviceInfo) =>
            {
                var item = this.container.Resolve<UsbCameraDeviceListItemViewModel>();
                item.DeviceName.Value = cameraDeviceInfo.Name;

                this.CameraDeviceListSource.Add(item);
            };
            //this.wsDiscoveryClient = new WsDiscoveryClient(cameraDiscoveryedAction);
            //this.wsDiscoveryClient.FindNetworkVideoTransmitterAsync();

            this.SelectionChangedCameraDeviceList = new ReactiveCommand<SelectionChangedEventArgs>()
                .WithSubscribe((item) => this.onSelectionChangedCameraDeviceList(item))
                .AddTo(this.disposable);
        }

        public ReactiveCollection<UsbCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<UsbCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveCommand<SelectionChangedEventArgs> SelectionChangedCameraDeviceList { get; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        private void onSelectionChangedCameraDeviceList(SelectionChangedEventArgs args)
        {
            Console.WriteLine($"Call onSelectionChangedCameraDeviceList() method. [args:{args}]");

            if (this.CameraDeviceSelectedItem.Value != null)
            {
                //    // カメラ接続に使用するログイン名／パスワードを取得する
                //    var dbAccessor = this.container.Resolve<DatabaseAccesser>();

                //    var cameraLoginInfo = dbAccessor.GetCameraLoginInfo();
                //    var userName = string.Empty;
                //    var password = string.Empty;

                //    if (cameraLoginInfo != null)
                //    {
                //        userName = cameraLoginInfo.Name;
                //        password = cameraLoginInfo.Password;
                //    }

                //    try
                //    {
                //        // 接続するカメラのIPアドレスを取得する
                //        var ipAddress = this.CameraDeviceSelectedItem.Value.IpAddress.Value;
                //        var task = GetCameraDeviceInfo(ipAddress, userName, password);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine(ex);
                //    }
            }
        }
    }
}
