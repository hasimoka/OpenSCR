using HalationGhost.WinApps;
using Mictlanix.DotNet.Onvif;
using Mictlanix.DotNet.Onvif.Common;
using OpenSCRLib;
using OptionViews.CameraLoginSettings;
using OptionViews.CameraLogoutSettings;
using OptionViews.Models;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OptionViews.AdvancedCameraSettings
{
    class IpCameraListPanelViewModel : HalationGhostViewModelBase, INavigationAware
    {
        private IContainerProvider container;

        private WsDiscoveryClient wsDiscoveryClient;

        public IpCameraListPanelViewModel(IRegionManager regionMan, IContainerProvider containerProvider): base(regionMan)
        {
            this.container = containerProvider;

            // ログイン状態に合わせたPanelを設定する
            var dbAccessor = this.container.Resolve<DatabaseAccesser>();
            var camearLoginInfo = dbAccessor.GetCameraLoginInfo();

            if (camearLoginInfo.IsLoggedIn)
            {
                regionMan.RegisterViewWithRegion("CameraLoginSettingRegion", typeof(CameraLogoutSettingPanel));
            }
            else
            {
                regionMan.RegisterViewWithRegion("CameraLoginSettingRegion", typeof(CameraLoginSettingPanel));
            }

            this.CameraDeviceListSource = new ReactiveCollection<IpCameraDeviceListItemViewModel>()
                .AddTo(this.disposable);

            this.CameraDeviceSelectedItem = new ReactiveProperty<IpCameraDeviceListItemViewModel>()
                .AddTo(this.disposable);

            Action<IpCameraDeviceInfo> cameraDiscoveryedAction = (cameraDeviceInfo) =>
            {
                var item = this.container.Resolve<IpCameraDeviceListItemViewModel>();
                item.DeviceName.Value = cameraDeviceInfo.Name;
                item.IpAddress.Value = cameraDeviceInfo.Address;
                item.EndpointUri = cameraDeviceInfo.EndpointUri;
                item.Location = cameraDeviceInfo.Location;

                this.CameraDeviceListSource.Add(item);

            };
            this.wsDiscoveryClient = new WsDiscoveryClient(cameraDiscoveryedAction);
            this.wsDiscoveryClient.FindNetworkVideoTransmitterAsync();

            this.SelectionChangedCameraDeviceList = new ReactiveCommand<SelectionChangedEventArgs>()
                .WithSubscribe((item) => this.onSelectionChangedCameraDeviceList(item))
                .AddTo(this.disposable);
        }

        public ReactiveCollection<IpCameraDeviceListItemViewModel> CameraDeviceListSource { get; }

        public ReactiveProperty<IpCameraDeviceListItemViewModel> CameraDeviceSelectedItem { get; set; }

        public ReactiveCommand<SelectionChangedEventArgs> SelectionChangedCameraDeviceList { get; }

        public bool IsNavigationTarget(NavigationContext navigationContext) { return true; }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.CameraDeviceListSource.Clear();
            this.wsDiscoveryClient.FindNetworkVideoTransmitterAsync();
        }

        private void onSelectionChangedCameraDeviceList(SelectionChangedEventArgs args)
        {
            Console.WriteLine($"Call onSelectionChangedCameraDeviceList() method. [args:{args}]");

            if (this.CameraDeviceSelectedItem.Value != null)
            {
                // カメラ接続に使用するログイン名／パスワードを取得する
                var dbAccessor = this.container.Resolve<DatabaseAccesser>();

                var cameraLoginInfo = dbAccessor.GetCameraLoginInfo();
                var userName = string.Empty;
                var password = string.Empty;

                if (cameraLoginInfo != null)
                {
                    userName = cameraLoginInfo.Name;
                    password = cameraLoginInfo.Password;
                }

                try
                {
                    // 接続するカメラのIPアドレスを取得する
                    var ipAddress = this.CameraDeviceSelectedItem.Value.IpAddress.Value;
                    var task = GetCameraDeviceInfo(ipAddress, userName, password);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private async Task GetCameraDeviceInfo(string host, string userName, string password)
        {
            var device = await OnvifClientFactory.CreateDeviceClientAsync(host, userName, password);
            var media = await OnvifClientFactory.CreateMediaClientAsync(host, userName, password);
            var streamSetup = new StreamSetup
            {
                Stream = StreamType.RTPUnicast,
                Transport = new Transport { Protocol = TransportProtocol.UDP }
            };

            //var ptz = await OnvifClientFactory.CreatePTZClientAsync(host, userName, password);
            //var imaging = await OnvifClientFactory.CreateImagingClientAsync(host, userName, password);
            //var caps = await device.GetCapabilitiesAsync(new CapabilityCategory[] { CapabilityCategory.All });
            bool absolute_move = false;
            bool relative_move = false;
            bool continuous_move = false;
            //bool focus = false;

            //Console.WriteLine("Capabilities");

            //Console.WriteLine("\tDevice: " + caps.Capabilities.Device.XAddr);
            //Console.WriteLine("\tEvents: " + caps.Capabilities.Events.XAddr);
            //Console.WriteLine("\tImaging: " + caps.Capabilities.Imaging.XAddr);
            //Console.WriteLine("\tMedia: " + caps.Capabilities.Media.XAddr);
            //Console.WriteLine("\tPTZ: " + caps.Capabilities.PTZ.XAddr);

            var profiles = await media.GetProfilesAsync();
            string profile_token = null;

            //Console.WriteLine("Profiles count :" + profiles.Profiles.Length);

            foreach (var profile in profiles.Profiles)
            {
                //Console.WriteLine($"Profile: {profile.token}");

                if (profile_token == null)
                {
                    profile_token = profile.token;
                    absolute_move = !string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultAbsolutePantTiltPositionSpace);
                    relative_move = !string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultRelativePanTiltTranslationSpace);
                    continuous_move = !string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultContinuousPanTiltVelocitySpace);
                }

                //Console.WriteLine($"\tTranslation Support");
                //Console.WriteLine($"\t\tAbsolute Translation: {!string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultAbsolutePantTiltPositionSpace)}");
                //Console.WriteLine($"\t\tRelative Translation: {!string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultRelativePanTiltTranslationSpace)}");
                //Console.WriteLine($"\t\tContinuous Translation: {!string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultContinuousPanTiltVelocitySpace)}");

                //if (!string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultRelativePanTiltTranslationSpace))
                //{
                //    var pan = profile.PTZConfiguration.PanTiltLimits.Range.XRange;
                //    var tilt = profile.PTZConfiguration.PanTiltLimits.Range.YRange;
                //    var zoom = profile.PTZConfiguration.ZoomLimits.Range.XRange;

                //    Console.WriteLine($"\tPan Limits: [{pan.Min}, {pan.Max}] Tilt Limits: [{tilt.Min}, {tilt.Max}] Tilt Limits: [{zoom.Min}, {zoom.Max}]");
                //}
            }

            foreach (var profile in profiles.Profiles)
            {
                try
                {
                    var mediaUri = await media.GetStreamUriAsync(streamSetup, profile.token).ConfigureAwait(false);
                    //Console.WriteLine($"使用 {profile.token} 获取直播地址 Uri={mediaUri.Uri} Timeout={mediaUri.Timeout}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetStreamUri throws exception. [{ex}]");
                }
            }

            //var configs = await ptz.GetConfigurationsAsync();

            //foreach (var config in configs.PTZConfiguration)
            //{
            //    Console.WriteLine($"PTZ Configuration: {config.token}");
            //}

            //var video_sources = await media.GetVideoSourcesAsync();
            //string vsource_token = null;

            //foreach (var source in video_sources.VideoSources)
            //{
            //    Console.WriteLine($"Video Source: {source.token}");

            //    if (vsource_token == null)
            //    {
            //        vsource_token = source.token;
            //        focus = source.Imaging.Focus != null;
            //    }

            //    Console.WriteLine($"\tFramerate: {source.Framerate}");
            //    Console.WriteLine($"\tResolution: {source.Resolution.Width}x{source.Resolution.Height}");

            //    Console.WriteLine($"\tFocus Settings");

            //    if (source.Imaging.Focus == null)
            //    {
            //        Console.WriteLine($"\t\tNone");
            //    }
            //    else
            //    {
            //        Console.WriteLine($"\t\tMode: {source.Imaging.Focus.AutoFocusMode}");
            //        Console.WriteLine($"\t\tNear Limit: {source.Imaging.Focus.NearLimit}");
            //        Console.WriteLine($"\t\tFar Limit: {source.Imaging.Focus.FarLimit}");
            //        Console.WriteLine($"\t\tDefault Speed: {source.Imaging.Focus.DefaultSpeed}");
            //    }

            //    Console.WriteLine($"\tExposure Settings");

            //    if (source.Imaging.Exposure == null)
            //    {
            //        Console.WriteLine($"\t\tNone");
            //    }
            //    else
            //    {
            //        Console.WriteLine($"\t\tMode: {source.Imaging.Exposure.Mode}");
            //        Console.WriteLine($"\t\tMin Iris: {source.Imaging.Exposure.MinIris}");
            //        Console.WriteLine($"\t\tMax Iris: {source.Imaging.Exposure.MaxIris}");
            //    }

            //    Console.WriteLine($"\tImage Settings");

            //    var imaging_opts = await imaging.GetOptionsAsync(source.token);

            //    Console.WriteLine($"\t\tBrightness: {source.Imaging.Brightness} [{imaging_opts.Brightness?.Min}, {imaging_opts.Brightness?.Max}]");
            //    Console.WriteLine($"\t\tColor Saturation: {source.Imaging.ColorSaturation} [{imaging_opts.ColorSaturation?.Min}, {imaging_opts.ColorSaturation?.Max}]");
            //    Console.WriteLine($"\t\tContrast: {source.Imaging.Contrast} [{imaging_opts.Contrast?.Min}, {imaging_opts.Contrast?.Max}]");
            //    Console.WriteLine($"\t\tSharpness: {source.Imaging.Sharpness} [{imaging_opts.Sharpness?.Min}, {imaging_opts.Sharpness?.Max}]");
            //}

            //var ptz_status = await ptz.GetStatusAsync(profile_token);

            //if (ptz_status.Position != null)
            //    Console.WriteLine($"Position: [{ptz_status.Position.PanTilt.x}, {ptz_status.Position.PanTilt.y}, {ptz_status.Position.Zoom.x}]");
            //if (ptz_status.MoveStatus != null)
            //    Console.WriteLine($"Pan/Tilt Status: {ptz_status.MoveStatus.PanTilt} Zoom Status: {ptz_status.MoveStatus.Zoom}");

            ////if (absolute_move)
            ////{
            ////    Console.WriteLine($"Absolute Move...");

            ////    try
            ////    {
            ////        //await ptz.AbsoluteMoveAsync(profile_token, new PTZVector
            ////        //{
            ////        //    PanTilt = new Vector2D
            ////        //    {
            ////        //        x = ptz_status.Position.PanTilt.x,
            ////        //        y = ptz_status.Position.PanTilt.y + 0.1f
            ////        //    },
            ////        //    Zoom = new Vector1D
            ////        //    {
            ////        //        x = 0f
            ////        //    }
            ////        //}, new PTZSpeed
            ////        //{
            ////        //    PanTilt = new Vector2D
            ////        //    {
            ////        //        x = 1f,
            ////        //        y = 1f
            ////        //    },
            ////        //    Zoom = new Vector1D
            ////        //    {
            ////        //        x = 0f
            ////        //    }
            ////        //});

            ////        await ptz.AbsoluteMoveAsync(profile_token, new PTZVector
            ////        {
            ////            PanTilt = new Vector2D
            ////            {
            ////                x = ptz_status.Position.PanTilt.x,
            ////                y = ptz_status.Position.PanTilt.y
            ////            },
            ////            Zoom = new Vector1D
            ////            {
            ////                x = ptz_status.Position.Zoom.x + 0.001f
            ////            }
            ////        }, new PTZSpeed
            ////        {
            ////            PanTilt = new Vector2D
            ////            {
            ////                x = 0f,
            ////                y = 0f
            ////            },
            ////            Zoom = new Vector1D
            ////            {
            ////                x = 1f
            ////            }
            ////        });

            ////        await Task.Delay(3000);

            ////        ptz_status = await ptz.GetStatusAsync(profile_token);

            ////        Console.WriteLine($"Position: [{ptz_status.Position.PanTilt.x}, {ptz_status.Position.PanTilt.y}, {ptz_status.Position.Zoom.x}]");
            ////        Console.WriteLine($"Pan/Tilt Status: {ptz_status.MoveStatus.PanTilt} Zoom Status: {ptz_status.MoveStatus.Zoom}");

            ////    }
            ////    catch (Exception ex)
            ////    {
            ////        Console.WriteLine($"{ex}");
            ////    }
            ////}

            ////if (relative_move)
            ////{
            ////    Console.WriteLine($"Relative Move...");

            ////    await ptz.RelativeMoveAsync(profile_token, new PTZVector
            ////    {
            ////        PanTilt = new Vector2D
            ////        {
            ////            x = 0.1f,
            ////            y = 0.1f
            ////        },
            ////        Zoom = new Vector1D
            ////        {
            ////            x = 0.1f
            ////        }
            ////    }, new PTZSpeed
            ////    {
            ////        PanTilt = new Vector2D
            ////        {
            ////            x = 0.1f,
            ////            y = 0.1f
            ////        },
            ////        Zoom = new Vector1D
            ////        {
            ////            x = 0.1f
            ////        }
            ////    });

            ////    await Task.Delay(3000);

            ////    ptz_status = await ptz.GetStatusAsync(profile_token);

            ////    Console.WriteLine($"Position: [{ptz_status.Position.PanTilt.x}, {ptz_status.Position.PanTilt.y}, {ptz_status.Position.Zoom.x}]");
            ////    Console.WriteLine($"Pan/Tilt Status: {ptz_status.MoveStatus.PanTilt} Zoom Status: {ptz_status.MoveStatus.Zoom}");
            ////}

            //if (continuous_move)
            //{
            //    Console.WriteLine($"Continuous Move...");

            //    await ptz.ContinuousMoveAsync(profile_token, new PTZSpeed
            //    {
            //        PanTilt = new Vector2D
            //        {
            //            x = 0,
            //            y = -0.5f
            //        },
            //        Zoom = new Vector1D
            //        {
            //            x = 0
            //        }
            //    }, null);

            //    await Task.Delay(1500);
            //    await ptz.StopAsync(profile_token, true, true);

            //    ptz_status = await ptz.GetStatusAsync(profile_token);

            //    Console.WriteLine($"Position: [{ptz_status.Position.PanTilt.x}, {ptz_status.Position.PanTilt.y}, {ptz_status.Position.Zoom.x}]");
            //    Console.WriteLine($"Pan/Tilt Status: {ptz_status.MoveStatus.PanTilt} Zoom Status: {ptz_status.MoveStatus.Zoom}");
            //}
        }
    }
}
