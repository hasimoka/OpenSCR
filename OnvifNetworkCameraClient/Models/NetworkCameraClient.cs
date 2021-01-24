using Reactive.Bindings;
using RtspClientSharp;
using OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames;
using OnvifNetworkCameraClient.Models.RawFramesReceiving;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Collections.Generic;
using Mictlanix.DotNet.Onvif;
using Mictlanix.DotNet.Onvif.Common;
using OpenSCRLib;
using Reactive.Bindings.Extensions;
using HalationGhost;

namespace OnvifNetworkCameraClient.Models
{
    public class NetworkCameraClient : BindableModelBase
    {
        private const string RTSP_PREFIX = "rtsp://";
        private const string HTTP_PREFIX = "http://";

        private readonly RealtimeVideoSource realtimeVideoSource;

        private IRawFramesSource rawFramesSource;

        private bool isRunning;

        public NetworkCameraClient()
        {
            this.realtimeVideoSource = new RealtimeVideoSource();

            // 初期表示用の画像を読み込む
            using (var stream = new MemoryStream(File.ReadAllBytes("Images/no_image_w.png")))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                this.FrameImage = new ReactivePropertySlim<BitmapSource>(bitmap)
                    .AddTo(this.Disposable);
            }

            this.realtimeVideoSource.FrameDecoded += OnFrameDecoded;

            this.isRunning = false;
        }

        public IVideoSource VideoSource => realtimeVideoSource;

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public void StartCapture(string mediaUrl, string loginName, string password)
        {
            if (mediaUrl == null)
                return;

            if (this.isRunning)
                this.StopCapture();

            if (rawFramesSource != null)
                return;

            string address = mediaUrl;

            if (!address.StartsWith(RTSP_PREFIX) && !address.StartsWith(HTTP_PREFIX))
                address = RTSP_PREFIX + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri))
                throw new Exception("Invalid device address.");

            var credential = new NetworkCredential(loginName, password);

            var connectionParameters = !string.IsNullOrEmpty(deviceUri.UserInfo) ? new ConnectionParameters(deviceUri) : new ConnectionParameters(deviceUri, credential);
            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

            this.rawFramesSource = new RawFramesSource(connectionParameters);
            this.realtimeVideoSource.SetRawFramesSource(this.rawFramesSource);

            this.rawFramesSource.Start();
            this.isRunning = true;
        }

        public void StopCapture()
        {
            if (this.rawFramesSource == null)
                return;

            this.rawFramesSource.Stop();
            this.realtimeVideoSource.SetRawFramesSource(null);
            this.rawFramesSource = null;

            this.isRunning = false;

            Task.Delay(TimeSpan.FromMilliseconds(100));
        }

        public async Task<List<OnvifNetworkCameraProfile>> GetCameraDeviceInfoAsync(string host, string userName, string password)
        {
            var results = new List<OnvifNetworkCameraProfile>();

            var media = await OnvifClientFactory.CreateMediaClientAsync(host, userName, password);
            var streamSetup = new StreamSetup
            {
                Stream = StreamType.RTPUnicast,
                Transport = new Transport { Protocol = TransportProtocol.UDP }
            };

            var profiles = await media.GetProfilesAsync();

            foreach (var profile in profiles.Profiles)
            {
                var cameraProfile = new OnvifNetworkCameraProfile();

                cameraProfile.ProfileToken = profile.token;
                if (profile.PTZConfiguration != null)
                {
                    cameraProfile.CanPtzAbsoluteMove = !string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultAbsolutePantTiltPositionSpace);
                    cameraProfile.CanPtzRelativeMove = !string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultRelativePanTiltTranslationSpace);
                    cameraProfile.CanPtzContinuousMove = !string.IsNullOrWhiteSpace(profile.PTZConfiguration.DefaultContinuousPanTiltVelocitySpace);
                }

                try
                {
                    var mediaUri = await media.GetStreamUriAsync(streamSetup, profile.token).ConfigureAwait(false);

                    cameraProfile.StreamUri = mediaUri.Uri;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetStreamUriAsync throws exception. [{ex}]");
                }

                try
                {
                    var videoEncoderConfig = await media.GetVideoEncoderConfigurationAsync(profile.VideoEncoderConfiguration.token);

                    cameraProfile.Encoding = videoEncoderConfig.Encoding;

                    cameraProfile.BitrateLimite = videoEncoderConfig.RateControl.BitrateLimit;
                    cameraProfile.EncodingInterval = videoEncoderConfig.RateControl.EncodingInterval;
                    cameraProfile.FrameRateLimit = videoEncoderConfig.RateControl.FrameRateLimit;

                    cameraProfile.VideoHeight = videoEncoderConfig.Resolution.Height;
                    cameraProfile.VideoWidth = videoEncoderConfig.Resolution.Width;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"GetVideoEncoderConfigurationAsync throws exception. [{ex}]");
                }

                results.Add(cameraProfile);
            }

            return results;
        }

        public async Task<bool> SetCameraDeviceInfoAsync(string host, string userName, string password, string profileToken, int bitrateLimit, int encodingInterval, int frameLateLimit)
        {
            var result = false;

            var media = await OnvifClientFactory.CreateMediaClientAsync(host, userName, password);
            var streamSetup = new StreamSetup
            {
                Stream = StreamType.RTPUnicast,
                Transport = new Transport { Protocol = TransportProtocol.UDP }
            };

            var profiles = await media.GetProfilesAsync();

            foreach (var profile in profiles.Profiles)
            {
                if (profile.token == profileToken)
                {
                    try
                    {
                        var videoEncoderConfig = await media.GetVideoEncoderConfigurationAsync(profile.VideoEncoderConfiguration.token);

                        videoEncoderConfig.RateControl.BitrateLimit = bitrateLimit;
                        videoEncoderConfig.RateControl.EncodingInterval = encodingInterval;
                        videoEncoderConfig.RateControl.FrameRateLimit = frameLateLimit;

                        await media.SetVideoEncoderConfigurationAsync(videoEncoderConfig, true);
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Throw exception. [{ex}]");
                    }
                }
            }

            return result;
        }

        public async Task MoveAsync(string host, string userName, string password, string profileToken, PtzDirection ptzDirection)
        {
            try
            {
                var ptz = await OnvifClientFactory.CreatePTZClientAsync(host, userName, password);

                var ptz_status = await ptz.GetStatusAsync(profileToken);

                switch (ptzDirection)
                {
                    case PtzDirection.UpMove:
                        await ptz.AbsoluteMoveAsync(profileToken, new PTZVector
                        {
                            PanTilt = new Vector2D
                            {
                                x = ptz_status.Position.PanTilt.x,
                                y = ptz_status.Position.PanTilt.y + 0.01f
                            },
                            Zoom = new Vector1D
                            {
                                x = ptz_status.Position.Zoom.x
                            }
                        }, new PTZSpeed
                        {
                            PanTilt = new Vector2D
                            {
                                x = 0f,
                                y = 1f
                            },
                            Zoom = new Vector1D
                            {
                                x = 0f
                            }
                        });
                        break;
                    case PtzDirection.DownMove:
                        await ptz.AbsoluteMoveAsync(profileToken, new PTZVector
                        {
                            PanTilt = new Vector2D
                            {
                                x = ptz_status.Position.PanTilt.x,
                                y = ptz_status.Position.PanTilt.y - 0.01f
                            },
                            Zoom = new Vector1D
                            {
                                x = ptz_status.Position.Zoom.x
                            }
                        }, new PTZSpeed
                        {
                            PanTilt = new Vector2D
                            {
                                x = 0f,
                                y = 1f
                            },
                            Zoom = new Vector1D
                            {
                                x = 0f
                            }
                        });
                        break;
                    case PtzDirection.LeftMove:
                        await ptz.AbsoluteMoveAsync(profileToken, new PTZVector
                        {
                            PanTilt = new Vector2D
                            {
                                x = ptz_status.Position.PanTilt.x + 0.01f,
                                y = ptz_status.Position.PanTilt.y
                            },
                            Zoom = new Vector1D
                            {
                                x = ptz_status.Position.Zoom.x
                            }
                        }, new PTZSpeed
                        {
                            PanTilt = new Vector2D
                            {
                                x = 1f,
                                y = 0f
                            },
                            Zoom = new Vector1D
                            {
                                x = 0f
                            }
                        });
                        break;
                    case PtzDirection.RightMove:
                        await ptz.AbsoluteMoveAsync(profileToken, new PTZVector
                        {
                            PanTilt = new Vector2D
                            {
                                x = ptz_status.Position.PanTilt.x - 0.01f,
                                y = ptz_status.Position.PanTilt.y
                            },
                            Zoom = new Vector1D
                            {
                                x = ptz_status.Position.Zoom.x
                            }
                        }, new PTZSpeed
                        {
                            PanTilt = new Vector2D
                            {
                                x = 1f,
                                y = 0f
                            },
                            Zoom = new Vector1D
                            {
                                x = 0f
                            }
                        });
                        break;
                    case PtzDirection.ZoomIn:
                        await ptz.AbsoluteMoveAsync(profileToken, new PTZVector
                        {
                            PanTilt = new Vector2D
                            {
                                x = ptz_status.Position.PanTilt.x,
                                y = ptz_status.Position.PanTilt.y
                            },
                            Zoom = new Vector1D
                            {
                                x = ptz_status.Position.Zoom.x + 0.004f
                            }
                        }, new PTZSpeed
                        {
                            PanTilt = new Vector2D
                            {
                                x = 0f,
                                y = 0f
                            },
                            Zoom = new Vector1D
                            {
                                x = 1f
                            }
                        });
                        break;
                    case PtzDirection.ZoomOut:
                        await ptz.AbsoluteMoveAsync(profileToken, new PTZVector
                        {
                            PanTilt = new Vector2D
                            {
                                x = ptz_status.Position.PanTilt.x,
                                y = ptz_status.Position.PanTilt.y
                            },
                            Zoom = new Vector1D
                            {
                                x = ptz_status.Position.Zoom.x - 0.004f
                            }
                        }, new PTZSpeed
                        {
                            PanTilt = new Vector2D
                            {
                                x = 0f,
                                y = 0f
                            },
                            Zoom = new Vector1D
                            {
                                x = 1f
                            }
                        });
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Throw exception: {ex}");
            }
        }

        public void ClearFrameImage()
        {
            // 初期表示用の画像を読み込む
            using (var stream = new MemoryStream(File.ReadAllBytes("Images/no_image_w.png")))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                this.FrameImage.Value = bitmap;
            }
        }

        /// <summary>
        /// 受信フレームのデコード完了後に呼び出されるコールバック関数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="decodedVideoFrame"></param>
        private void OnFrameDecoded(object sender, DecodedVideoFrame decodedVideoFrame)
        {
            // Bitmap -> BitmapImageへ変換をおこなう
            using (var memory = new MemoryStream())
            {
                decodedVideoFrame.Image.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                FrameImage.Value = bitmapImage;
            }
        }
    }
}
