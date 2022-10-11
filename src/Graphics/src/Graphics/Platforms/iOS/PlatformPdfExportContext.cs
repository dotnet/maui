using System;
using System.IO;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	internal class PlatformPdfExportContext : PdfExportContext
	{
		private string _tempFilePath;
		private readonly NSMutableDictionary _documentInfo;
		private readonly PlatformCanvas _canvas;
		private bool _closed;

		public PlatformPdfExportContext(
			float defaultWidth,
			float defaultHeight) : base(defaultWidth, defaultHeight)
		{
			_documentInfo = new NSMutableDictionary();
			_canvas = new PlatformCanvas(() => CGColorSpace.CreateDeviceRGB());
		}

		protected override void AddPageImpl(float width, float height)
		{
			if (_closed)
				throw new Exception("Unable to add a page because the PDFContext is already closed.");

			if (_tempFilePath == null)
			{
				_tempFilePath = Path.GetTempFileName();
				UIGraphics.BeginPDFContext(_tempFilePath, CGRect.Empty, _documentInfo);
			}

			var pageInfo = new NSMutableDictionary();
			UIGraphics.BeginPDFPage(new CGRect(0, 0, width, height), pageInfo);

			var context = UIGraphics.GetCurrentContext();
			context.SetFillColorSpace(CGColorSpace.CreateDeviceRGB());
			context.SetStrokeColorSpace(CGColorSpace.CreateDeviceRGB());
			_canvas.Context = context;
		}

		public override void WriteToStream(Stream stream)
		{
			Close();

			using (var inputStream = new FileStream(_tempFilePath, FileMode.Open, FileAccess.Read))
			{
				inputStream.CopyTo(stream);
			}
		}

		public NSData Data
		{
			get
			{
				Close();
				return NSData.FromFile(_tempFilePath);
			}
		}

		public override ICanvas Canvas => _canvas;

		private void Close()
		{
			if (!_closed)
			{
				try
				{
#if IOS16_0_OR_GREATER
					UIGraphics.EndPDFContext();
#else
#pragma warning disable CS0618 // Type or member is obsolete
					UIGraphics.EndPDFContent();
#pragma warning restore CS0618 // Type or member is obsolete
#endif
				}
				catch (Exception exc)
				{
					System.Diagnostics.Debug.WriteLine(exc);
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
				if (File.Exists(_tempFilePath))
					File.Delete(_tempFilePath);
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc);
			}
		}
	}
}
