using DirectShowLib;
using HalationGhost;
using OpenSCRLib;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DirectShowUsbCameraClient
{
    public class DirectShowCaputer : BindableModelBase, ISampleGrabberCB, IDisposable
    {
        #region Member variables

        private const double DPI = 96.0d;

        /// <summary> graph builder interface. </summary>
        private IFilterGraph2 filterGraph;

        // Used to snap picture on Still pin
        private IAMVideoControl videoControl;
        private IPin pinStill;

        // <summary> buffer for bitmap data.Always release by caller</summary>
        private IntPtr ipBuffer;

        #endregion

        #region APIs

        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] int Length);

        #endregion

        public DirectShowCaputer()
        {
            this.filterGraph = null;

            this.videoControl = null;
            this.pinStill = null;

            this.ipBuffer = IntPtr.Zero;

            this.FrameImage = new ReactivePropertySlim<BitmapSource>()
                .AddTo(this.Disposable);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Stride { get; set; }

        public static List<DsDevice> GetCaptureDevices()
        {
            DsDevice[] capDevices;

            // Get the collection of video devices
            capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            return capDevices.ToList<DsDevice>();
        }

        public static List<UsbCameraVideoInfo> GetVideoInfos(UsbCameraDeviceInfo captureDevice)
        {
            List<UsbCameraVideoInfo> results = new List<UsbCameraVideoInfo>();

            int hr;

            IBaseFilter captureFilter = null;
            IPin pinStill = null;

            // Get the graphbuilder object
            var filterGraph = new FilterGraph() as IFilterGraph2;

            try
            {
                IMoniker moniker = null;
                var devices = DirectShowCaputer.GetCaptureDevices();
                foreach (var device in devices)
                {
                    if (device.DevicePath == captureDevice.DevicePath)
                    {
                        moniker = device.Mon;
                    }
                }

                // add the video input device
                hr = filterGraph.AddSourceFilterForMoniker(moniker, null, captureDevice.Name, out captureFilter);
                DsError.ThrowExceptionForHR(hr);

                // Find the still pin
                pinStill = DsFindPin.ByCategory(captureFilter, PinCategory.Still, 0);

                // Didn't find one.  Is there a preview pin?
                if (pinStill == null)
                {
                    pinStill = DsFindPin.ByCategory(captureFilter, PinCategory.Preview, 0);
                }

                // Still haven't found one.  Need to put a splitter in so we have
                // one stream to capture the bitmap from, and one to display.  Ok, we
                // don't *have* to do it that way, but we are going to anyway.
                if (pinStill == null)
                {
                    IPin pinRaw = null;

                    // Add a splitter
                    var smartTee = (IBaseFilter)new SmartTee();

                    try
                    {
                        hr = filterGraph.AddFilter(smartTee, "SmartTee");
                        DsError.ThrowExceptionForHR(hr);

                        // Find the find the capture pin from the video device and the
                        // input pin for the splitter, and connnect them
                        pinRaw = DsFindPin.ByCategory(captureFilter, PinCategory.Capture, 0);
                        IAMStreamConfig videoStreamConfig = pinRaw as IAMStreamConfig;

                        results = DirectShowCaputer.GetStreamCapabilities(videoStreamConfig);
                    }
                    finally
                    {
                        if (pinRaw != null)
                        {
                            Marshal.ReleaseComObject(pinRaw);
                        }
                        if (pinRaw != smartTee)
                        {
                            Marshal.ReleaseComObject(smartTee);
                        }
                    }
                }
                else
                {
                    IAMStreamConfig videoStreamConfig = pinStill as IAMStreamConfig;

                    results = DirectShowCaputer.GetStreamCapabilities(videoStreamConfig);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return results;
        }

        private static List<UsbCameraVideoInfo> GetStreamCapabilities(IAMStreamConfig videoStreamConfig)
        {
            var results = new List<UsbCameraVideoInfo>();

            int hr;

            int capabilityCount;
            int capabilitySize;
            hr = videoStreamConfig.GetNumberOfCapabilities(out capabilityCount, out capabilitySize);
            DsError.ThrowExceptionForHR(hr);

            IntPtr taskMemPointer = IntPtr.Zero;
            AMMediaType mediaType = null;

            try
            {
                taskMemPointer = Marshal.AllocCoTaskMem(capabilitySize);

                for (int i = 0; i < capabilityCount; i++)
                {
                    hr = videoStreamConfig.GetStreamCaps(i, out mediaType, taskMemPointer);
                    DsError.ThrowExceptionForHR(hr);

                    var videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
                    if (videoInfo.BmiHeader.Width != 0 && videoInfo.BmiHeader.Height != 0 && videoInfo.BmiHeader.BitCount != 0)
                        results.Add(new UsbCameraVideoInfo(
                            videoInfo.BmiHeader.Width, videoInfo.BmiHeader.Height, videoInfo.BmiHeader.BitCount));
                }
            }
            finally
            {
                if (taskMemPointer != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(taskMemPointer);
                }
                if (mediaType != null)
                {
                    DsUtils.FreeAMMediaType(mediaType);
                }
            }

            return results;
        }

        public int SampleCB(double sampleTime, IMediaSample mediaSample)
        {
            Marshal.ReleaseComObject(mediaSample);
            return 0;
        }

        public int BufferCB(double sampleTime, IntPtr buffer, int bufferLength)
        {
            // Save the buffer
            CopyMemory(ipBuffer, buffer, bufferLength);

            var bitmapSource = BitmapSource.Create(this.Width, this.Height, DPI, DPI, PixelFormats.Bgr24, null, this.ipBuffer, bufferLength, this.Stride);

            // 画像を180度回転する
            var transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = bitmapSource;
            transformedBitmap.Transform = new RotateTransform(180);
            transformedBitmap.EndInit();

            // 非GUIスレッドで作成した画像をGUIスレッドで表示するためのおまじない
            transformedBitmap.Freeze();

            this.FrameImage.Value = transformedBitmap;

            return 0;
        }

        public new void Dispose()
        {
            base.Dispose();

            this.CloseInterfaces();
        }

        public void Start(string devicePath, int width, int height, short bitCount)
        {
            try
            {
                var devices = DirectShowCaputer.GetCaptureDevices();
                foreach(var device in devices)
                {
                    if (device.DevicePath == devicePath)
                    {
                        // Set up the capture graph
                        this.SetupGraph(device.Mon, device.Name, width, height, bitCount);
                    }
                }
            }
            catch
            {
                this.CloseInterfaces();
                throw;
            }
        }

        public void Stop()
        {
            this.CloseInterfaces();
        }

        /// <summary> build the capture graph for grabber. </summary>
        private void SetupGraph(IMoniker moniker, string deviceName, int width, int height, short bitCount)
        {
            int hr;

            ISampleGrabber sampleGrabber = null;
            IBaseFilter captureFilter = null;
            //IPin pinCaptureOut = null;
            IPin pinSampleIn = null;
            //IPin pinRenderIn = null;

            // Get the graphbuilder object
            this.filterGraph = new FilterGraph() as IFilterGraph2;

            try
            {
                // add the video input device
                hr = this.filterGraph.AddSourceFilterForMoniker(moniker, null, deviceName, out captureFilter);
                DsError.ThrowExceptionForHR(hr);

                // Find the still pin
                this.pinStill = DsFindPin.ByCategory(captureFilter, PinCategory.Still, 0);

                // Didn't find one.  Is there a preview pin?
                if (this.pinStill == null)
                {
                    this.pinStill = DsFindPin.ByCategory(captureFilter, PinCategory.Preview, 0);
                }

                // Still haven't found one.  Need to put a splitter in so we have
                // one stream to capture the bitmap from, and one to display.  Ok, we
                // don't *have* to do it that way, but we are going to anyway.
                if (this.pinStill == null)
                {
                    IPin pinCapture = null;
                    IPin pinSmartTee = null;

                    // There is no still pin
                    this.videoControl = null;

                    // Add a splitter
                    IBaseFilter filterSmartTee = (IBaseFilter)new SmartTee();

                    try
                    {
                        hr = this.filterGraph.AddFilter(filterSmartTee, "SmartTee");
                        DsError.ThrowExceptionForHR(hr);

                        // Find the find the capture pin from the video device and the
                        // input pin for the splitter, and connnect them
                        pinCapture = DsFindPin.ByCategory(captureFilter, PinCategory.Capture, 0);
                        pinSmartTee = DsFindPin.ByDirection(filterSmartTee, PinDirection.Input, 0);

                        hr = this.filterGraph.Connect(pinCapture, pinSmartTee);
                        DsError.ThrowExceptionForHR(hr);

                        // Now set the capture and still pins (from the splitter)
                        this.pinStill = DsFindPin.ByName(filterSmartTee, "Capture");
                        //pinCaptureOut = DsFindPin.ByName(filterSmartTee, "Preview");

                        // If any of the default config items are set, perform the config
                        // on the actual video device (rather than the splitter)
                        if (height + width + bitCount > 0)
                        {
                            this.SetConfigParms(pinCapture, width, height, bitCount);
                        }
                    }
                    finally
                    {
                        if (pinCapture != null)
                        {
                            Marshal.ReleaseComObject(pinCapture);
                        }
                        if (pinCapture != pinSmartTee)
                        {
                            Marshal.ReleaseComObject(pinSmartTee);
                        }
                        if (pinCapture != filterSmartTee)
                        {
                            Marshal.ReleaseComObject(filterSmartTee);
                        }
                    }
                }
                else
                {
                    // Get a control pointer (used in Click())
                    this.videoControl = captureFilter as IAMVideoControl;

                    // If any of the default config items are set
                    if (height + width + bitCount > 0)
                    {
                        this.SetConfigParms(this.pinStill, width, height, bitCount);
                    }
                }

                // Get the SampleGrabber interface
                sampleGrabber = new SampleGrabber() as ISampleGrabber;

                // Configure the sample grabber
                IBaseFilter baseGrabFlt = sampleGrabber as IBaseFilter;
                this.ConfigureSampleGrabber(sampleGrabber);
                pinSampleIn = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Input, 0);

                // Add the sample grabber to the graph
                hr = this.filterGraph.AddFilter(baseGrabFlt, "Ds.NET Grabber");
                DsError.ThrowExceptionForHR(hr);

                if (this.videoControl == null)
                {
                    // Connect the Still pin to the sample grabber
                    hr = this.filterGraph.Connect(this.pinStill, pinSampleIn);
                    DsError.ThrowExceptionForHR(hr);
                }
                else
                {
                    // Connect the Still pin to the sample grabber
                    hr = this.filterGraph.Connect(this.pinStill, pinSampleIn);
                    DsError.ThrowExceptionForHR(hr);
                }

                // Learn the video properties
                SaveSizeInfo(sampleGrabber);

                // Start the graph
                IMediaControl mediaCtrl = this.filterGraph as IMediaControl;
                hr = mediaCtrl.Run();
                DsError.ThrowExceptionForHR(hr);
            }
            finally
            {
                if (sampleGrabber != null)
                {
                    Marshal.ReleaseComObject(sampleGrabber);
                    sampleGrabber = null;
                }
                if (pinSampleIn != null)
                {
                    Marshal.ReleaseComObject(pinSampleIn);
                    pinSampleIn = null;
                }
            }
        }

        private void SaveSizeInfo(ISampleGrabber sampGrabber)
        {
            int hr;

            // Get the media type from the SampleGrabber
            AMMediaType media = new AMMediaType();

            hr = sampGrabber.GetConnectedMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("Unknown Grabber Media Format");
            }

            // Grab the size info
            VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
            this.Width = videoInfoHeader.BmiHeader.Width;
            this.Height = videoInfoHeader.BmiHeader.Height;
            this.Stride = this.Width * (videoInfoHeader.BmiHeader.BitCount / 8);

            this.ipBuffer = Marshal.AllocCoTaskMem(Math.Abs(this.Stride) * this.Height);

            DsUtils.FreeAMMediaType(media);
            media = null;
        }

        /// <summary>
        /// フィルタ内を通るサンプルをバッファにコピーする設定をおこなう。
        /// サンプル時のコールバック設定をおこなう。
        /// </summary>
        /// <param name="sampGrabber"></param>
        private void ConfigureSampleGrabber(ISampleGrabber sampGrabber)
        {
            int hr;
            AMMediaType media = new AMMediaType();

            // Set the media type to Video/RBG24
            media.majorType = MediaType.Video;
            media.subType = MediaSubType.RGB24;
            media.formatType = FormatType.VideoInfo;
            hr = sampGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);
            media = null;

            // Configure the samplegrabber
            hr = sampGrabber.SetCallback(this, 1);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// Set the Framerate, and video size.
        /// </summary>
        /// <param name="pinStill"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bitCount"></param>
        private void SetConfigParms(IPin pinStill, int width, int height, short bitCount)
        {
            int hr;
            AMMediaType media;
            VideoInfoHeader v;

            IAMStreamConfig videoStreamConfig = pinStill as IAMStreamConfig;

            // Get the existing format block
            hr = videoStreamConfig.GetFormat(out media);
            DsError.ThrowExceptionForHR(hr);

            try
            {
                // copy out the videoinfoheader
                v = new VideoInfoHeader();
                Marshal.PtrToStructure(media.formatPtr, v);

                // if overriding the width, set the width
                if (width > 0)
                {
                    v.BmiHeader.Width = width;
                }

                // if overriding the Height, set the Height
                if (height > 0)
                {
                    v.BmiHeader.Height = height;
                }

                // if overriding the bits per pixel
                if (bitCount > 0)
                {
                    v.BmiHeader.BitCount = bitCount;
                }

                // Copy the media structure back
                Marshal.StructureToPtr(v, media.formatPtr, false);

                // Set the new format
                hr = videoStreamConfig.SetFormat(media);
                DsError.ThrowExceptionForHR(hr);
            }
            finally
            {
                DsUtils.FreeAMMediaType(media);
                media = null;
            }
        }

        /// <summary> Shut down capture </summary>
        private void CloseInterfaces()
        {
            int hr;

            try
            {
                if (this.filterGraph != null)
                {
                    IMediaControl mediaCtrl = this.filterGraph as IMediaControl;

                    // Stop the graph
                    hr = mediaCtrl.Stop();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            if (this.filterGraph != null)
            {
                Marshal.ReleaseComObject(this.filterGraph);
                this.filterGraph = null;
            }

            if (this.videoControl != null)
            {
                Marshal.ReleaseComObject(this.videoControl);
                this.videoControl = null;
            }

            if (this.pinStill != null)
            {
                Marshal.ReleaseComObject(this.pinStill);
                this.pinStill = null;
            }

            if (this.ipBuffer != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.ipBuffer);
                this.ipBuffer = IntPtr.Zero;
            }
        }
    }
}
