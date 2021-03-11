using System;
using System.IO;
using CoreGraphics;
using Foundation;

namespace Microsoft.Maui.Graphics.CoreGraphics
{
    public class NativePdfExportContext : PdfExportContext
    {
        private NSMutableDictionary _documentInfo;
        private NSMutableData _data;
        private CGContextPDF _context;
        private readonly NativeCanvas _canvas;
        private bool _closed;
        private bool _pageOpen;

        public NativePdfExportContext(
            float defaultWidth,
            float defaultHeight) : base(defaultWidth, defaultHeight)
        {
            _documentInfo = new NSMutableDictionary();
            _canvas = new NativeCanvas(() => CGColorSpace.CreateDeviceRGB());
        }

        protected override void AddPageImpl(float width, float height)
        {
            if (_closed)
                throw new Exception("Unable to add a page because the PDFContext is already closed.");

            if (_data == null)
            {
                _data = new NSMutableData();
                var consumer = new CGDataConsumer(_data);
                _context = new CGContextPDF(consumer, CGRect.Empty, null);
                _context.SetFillColorSpace(CGColorSpace.CreateDeviceRGB());
                _context.SetStrokeColorSpace(CGColorSpace.CreateDeviceRGB());
            }

            if (_pageOpen)
                _context.EndPage();

            _context.BeginPage(new CGRect(0, 0, width, height));
            _context.TranslateCTM(0, height);
            _context.ScaleCTM(1, -1);
            _context.SetLineWidth(1);
            _context.SetFillColor(new CGColor(1, 1));
            _context.SetStrokeColor(new CGColor(0, 1));

            _pageOpen = true;

            _canvas.Context = _context;
        }

        public override void WriteToStream(Stream stream)
        {
            Close();

            if (_data != null)
            {
                using (var inputStream = _data.AsStream())
                {
                    inputStream.CopyTo(stream);
                }
            }
        }

        public NSData Data
        {
            get
            {
                Close();
                return _data;
            }
        }

        public override ICanvas Canvas => _canvas;

        private void Close()
        {
            if (!_closed)
            {
                try
                {
                    if (_pageOpen)
                        _context.EndPage();

                    _context.Close();
                }
                catch (Exception exc)
                {
                    Logger.Warn(exc);
                }
                finally
                {
                    _closed = true;
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Close();

            try
            {
                _canvas?.Dispose();
                _context?.Dispose();
            }
            catch (Exception exc)
            {
                Logger.Warn(exc);
            }
        }
    }
}