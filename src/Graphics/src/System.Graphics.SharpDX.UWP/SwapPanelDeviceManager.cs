using Windows.UI.Xaml.Controls;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.DXGI.AlphaMode;

namespace System.Graphics.SharpDX
{
    public class SwapPanelDeviceManager : DeviceManager
    {
        private readonly SwapChainPanel panel;
        private Bitmap1 d2dTargetBitmap;
        private SwapChain1 swapChain;

        public SwapPanelDeviceManager(SwapChainPanel panel)
        {
            this.panel = panel;
        }

        public bool Initialized => swapChain != null;

        public override void Initialize(float dpi)
        {
            base.Initialize(dpi);
            CreateSizeDependentResources();
        }

        protected virtual void CreateSizeDependentResources()
        {
            // Ensure dependent objects have been released.
            d2dContext.Target = null;
            if (d2dTargetBitmap != null) RemoveAndDispose(ref d2dTargetBitmap);
            d3dContext.OutputMerger.SetRenderTargets((RenderTargetView)null);
            d3dContext.Flush();
            /*d3dContext.FinishCommandList(false);
            d3dContext.li*/

            // Set render target size to the rendered size of the panel including the composition scale, 
            // defaulting to the minimum of 1px if no size was specified.
            var scale = Dpi / 96;
            var renderTargetWidth = (int)(panel.ActualWidth * scale);
            var renderTargetHeight = (int)(panel.ActualHeight * scale);
            
            if (renderTargetWidth == 0 || renderTargetHeight == 0) return;

            if (swapChain != null)
            {
                swapChain.ResizeBuffers(2, renderTargetWidth, renderTargetHeight, Format.B8G8R8A8_UNorm, SwapChainFlags.None);
            }
            else
            {
                var swapChainDesc = new SwapChainDescription1();
                swapChainDesc.Width = renderTargetWidth;
                swapChainDesc.Height = renderTargetHeight;
                swapChainDesc.Format = Format.B8G8R8A8_UNorm;
                swapChainDesc.Stereo = new RawBool(false);
                swapChainDesc.SampleDescription.Count = 1;
                swapChainDesc.SampleDescription.Quality = 0;
                swapChainDesc.Usage = Usage.RenderTargetOutput;
                swapChainDesc.BufferCount = 2;
                swapChainDesc.SwapEffect = SwapEffect.FlipSequential;
                swapChainDesc.Flags = SwapChainFlags.None;
                swapChainDesc.AlphaMode = AlphaMode.Unspecified;  // Todo... May need to change
                //swapChainDesc.Scaling = Scaling.None;

                using (var dxgiDevice = d3dDevice.QueryInterface<global::SharpDX.DXGI.Device1>())
                {
                    var dxgiAdapter = dxgiDevice.Adapter;
                    var dxgiFactory = dxgiAdapter.GetParent<global::SharpDX.DXGI.Factory2>();

                    swapChain = Collect(new SwapChain1(dxgiFactory, d3dDevice, ref swapChainDesc, null));

                    // Counter act the composition scale of the render target as   
                    // we already handle this in the platform window code.   
                    using (var swapChain2 = swapChain.QueryInterface<SwapChain2>())
                    {
                        var inverseScale = new RawMatrix3x2();
                        inverseScale.M11 = 1.0f / scale;
                        inverseScale.M22 = 1.0f / scale;
                        swapChain2.MatrixTransform = inverseScale;
                    }
                    
                    dxgiDevice.MaximumFrameLatency = 1;
                }

                // Associate the SwapChainBackgroundPanel with the swap chain
                using (var panelNative = ComObject.As<ISwapChainPanelNative>(panel))
                {
                    panelNative.SwapChain =swapChain;
                }
            }

            var bitmapProperties = new BitmapProperties1(
                new PixelFormat(Format.B8G8R8A8_UNorm, global::SharpDX.Direct2D1.AlphaMode.Premultiplied),
                Dpi,
                Dpi,
                BitmapOptions.Target | BitmapOptions.CannotDraw);

            using (var dxgiBackBuffer = swapChain.GetBackBuffer<Surface>(0))
            {
                d2dTargetBitmap = Collect(new Bitmap1(d2dContext, dxgiBackBuffer, bitmapProperties));
                d2dContext.Target = d2dTargetBitmap;
            }
        }

        public void OnSizeChange(double width, double height)
        {
            CreateSizeDependentResources();
        }

        public void Present()
        {
            if (swapChain != null)
            {
                swapChain.Present(0, PresentFlags.None);
            }
        }
    }
}
