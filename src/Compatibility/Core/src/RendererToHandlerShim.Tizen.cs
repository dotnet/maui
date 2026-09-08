#pragma warning disable CS0612 // Type or member is obsolete
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using static Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Platform;
#pragma warning disable CS0612 // Type or member is obsolete
using Microsoft.Maui.Graphics;
using PlatformView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility
{
	public partial class RendererToHandlerShim : IPlatformViewHandler
	{
		protected override PlatformView CreatePlatformView()
		{
			return VisualElementRenderer.NativeView;
		}

		IVisualElementRenderer CreateRenderer(IView view)
		{
			return Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view) ?? new DefaultRenderer();
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (VisualElementRenderer == null)
				return Size.Zero;

			widthConstraint = widthConstraint < 0 ? double.PositiveInfinity : widthConstraint;
			heightConstraint = heightConstraint < 0 ? double.PositiveInfinity : heightConstraint;
			return VisualElementRenderer.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void UpdateValue(string property)
		{
			base.UpdateValue(property);
			if (property == "Frame")
			{
				PlatformArrange(VisualElementRenderer.Element.Bounds);
			}
		}

		public override bool NeedsContainer => false;

		public override void PlatformArrange(Rect frame)
		{
			base.PlatformArrange(frame);
			VisualElementRenderer.UpdateLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				VisualElementRenderer?.Dispose();
				VisualElementRenderer = null;
			}
			base.Dispose(disposing);
		}
	}
}
