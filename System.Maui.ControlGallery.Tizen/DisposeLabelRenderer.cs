using System.Maui;
using System.Maui.Controls;
using System.Maui.ControlGallery.Tizen;
using System.Maui.Platform.Tizen;

[assembly: ExportRenderer(typeof(DisposeLabel), typeof(DisposeLabelRenderer))]
namespace System.Maui.ControlGallery.Tizen
{
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