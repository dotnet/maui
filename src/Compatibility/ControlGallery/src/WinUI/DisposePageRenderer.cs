using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: ExportRenderer(typeof(DisposePage), typeof(DisposePageRenderer))]
[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
#pragma warning restore CS0612 // Type or member is obsolete

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	[System.Obsolete]
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

	[System.Obsolete]
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