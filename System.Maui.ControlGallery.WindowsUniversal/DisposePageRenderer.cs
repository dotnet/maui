using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Controls;
using System.Maui.Platform.UWP;

[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]

namespace System.Maui.ControlGallery.WindowsUniversal
{
	public class DisposePageRenderer : PageRenderer
	{
		bool _disposed;

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				((DisposePage)Element).SendRendererDisposed();
			}

			base.Dispose(disposing);

		}
	}

	public class DisposeLabelRenderer : LabelRenderer
	{
		protected override void Dispose(bool disposing)
		{

			if (disposing)
			{
				((DisposeLabel)Element).SendRendererDisposed();
			}
			base.Dispose(disposing);
		}
	}
}