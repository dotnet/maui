using System.IO;
using SharpDX;
using SharpDX.Direct3D;
using d2 = SharpDX.Direct2D1;
using d3d = SharpDX.Direct3D11;
using dxgi = SharpDX.DXGI;
using wic = SharpDX.WIC;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public class DXBitmapExportContext : BitmapExportContext
    {
        private readonly d2.PixelFormat _pixelFormat;
        private readonly bool _disposeBitmap;
        private d2.Bitmap1 _bitmap;
        private DXCanvas _canvas;
        private bool _contextOpen = true;

        private static d3d.Device _default3DDevice;
        private static d3d.Device1 _default3DDevice1;
        private static d2.Device _device2D;
        private static d2.DeviceContext _deviceContext;
        private static dxgi.Device _dxgiDevice;

        public DXBitmapExportContext(int width, int height, float displayScale = 1, int dpi = 72, bool disposeBitmap = true)
            : base(width, height, dpi)
        {
            _disposeBitmap = disposeBitmap;

            if (_default3DDevice == null)
            {
                _default3DDevice = new d3d.Device(DriverType.Hardware, d3d.DeviceCreationFlags.VideoSupport | d3d.DeviceCreationFlags.BgraSupport);
                // default3DDevice = new d3d.Device(DriverType.Warp,  d3d.DeviceCreationFlags.BgraSupport);

                _default3DDevice1 = _default3DDevice.QueryInterface<d3d.Device1>();
                _dxgiDevice = _default3DDevice1.QueryInterface<dxgi.Device>(); // get a reference to DXGI device

                _device2D = new d2.Device(_dxgiDevice);

                // initialize the DeviceContext - it will be the D2D render target
                _deviceContext = new d2.DeviceContext(_device2D, d2.DeviceContextOptions.None);
            }

            // specify a pixel format that is supported by both and WIC
            _pixelFormat = new d2.PixelFormat(dxgi.Format.B8G8R8A8_UNorm, d2.AlphaMode.Premultiplied);

            // create the d2d bitmap description using default flags
            var bitmapProps = new d2.BitmapProperties1(_pixelFormat, Dpi, Dpi,
                d2.BitmapOptions.Target | d2.BitmapOptions.CannotDraw);

            // create our d2d bitmap where all drawing will happen
            _bitmap = new d2.Bitmap1(_deviceContext, new Size2(width, height), bitmapProps);

            // associate bitmap with the d2d context
            _deviceContext.Target = _bitmap;
            _deviceContext.BeginDraw();
            _deviceContext.Clear(new Color4 {Alpha = 0, Blue = 0, Green = 0, Red = 0});

            _canvas = new DXCanvas(_deviceContext) {DisplayScale = displayScale};
        }

        public override ICanvas Canvas => _canvas;

        public d2.RenderTarget DeviceContext => _deviceContext;

        public override void Dispose()
        {
            if (_canvas != null)
            {
                _canvas.Dispose();
                _canvas = null;
            }

            if (_bitmap != null && _disposeBitmap)
            {
                _bitmap.Dispose();
                _bitmap = null;
            }

            base.Dispose();
        }

        public void Shutdown()
        {
            if (_deviceContext != null)
            {
                _deviceContext.Dispose();
                _deviceContext = null;
            }

            if (_device2D != null)
            {
                _device2D.Dispose();
                _device2D = null;
            }

            if (_dxgiDevice != null)
            {
                _dxgiDevice.Dispose();
                _dxgiDevice = null;
            }

            if (_default3DDevice1 != null)
            {
                _default3DDevice1.Dispose();
                _default3DDevice1 = null;
            }

            if (_default3DDevice != null)
            {
                _default3DDevice.Dispose();
                _default3DDevice = null;
            }
        }

        public override IImage Image
        {
            get
            {
                if (_contextOpen)
                {
                    _deviceContext.EndDraw();
                    _contextOpen = false;
                }

                return new DXImage(_bitmap);
            }
        }


        public override void WriteToStream(Stream stream)
        {
            if (_contextOpen)
            {
                _deviceContext.EndDraw();
                _contextOpen = false;
            }

            // use the appropriate overload to write either to stream or to a file
            using (var wicStream = new wic.WICStream(DXGraphicsService.FactoryImaging, stream))
            {
                // select the image encoding format HERE
                using (var vEncoder = new wic.PngBitmapEncoder(DXGraphicsService.FactoryImaging))
                {
                    WriteToEncoder(vEncoder, wicStream);
                }
            }
        }

        private void WriteToEncoder(wic.BitmapEncoder encoder, wic.WICStream wicStream)
        {
            encoder.Initialize(wicStream);

            using (var bitmapFrameEncode = new wic.BitmapFrameEncode(encoder))
            {
                bitmapFrameEncode.Initialize();
                bitmapFrameEncode.SetSize(Width, Height);
                var format = wic.PixelFormat.FormatDontCare;
                bitmapFrameEncode.SetPixelFormat(ref format);

                // this is the trick to write D2D1 bitmap to WIC
                var imageEncoder = new wic.ImageEncoder(DXGraphicsService.FactoryImaging, _device2D);
                var parameters = new wic.ImageParameters(_pixelFormat, Dpi, Dpi, 0, 0, Width, Height);
                imageEncoder.WriteFrame(_bitmap, bitmapFrameEncode, parameters);

                bitmapFrameEncode.Commit();
                encoder.Commit();
            }
        }
    }
}