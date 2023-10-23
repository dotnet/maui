#pragma warning disable CS0612 // Type or member is obsolete
using Microsoft.Maui.Controls.Compatibility.Platform.Gtk;
#pragma warning disable CS0612 // Type or member is obsolete
using Microsoft.Maui.Graphics;
using Rect = Microsoft.Maui.Graphics.Rect;
using PlatformView = Gtk.Widget;

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

		public override void PlatformArrange(Rect frame)
		{
			base.PlatformArrange(frame);
			VisualElementRenderer.UpdateLayout();
		}

		public void SetRenderer(VisualElement view, IVisualElementRenderer visualElementRenderer)
		{
		}

	}

}