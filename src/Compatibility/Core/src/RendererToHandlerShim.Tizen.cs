#pragma warning disable CS0612 // Type or member is obsolete
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
#pragma warning disable CS0612 // Type or member is obsolete
using Microsoft.Maui.Graphics;
using Rect = Microsoft.Maui.Graphics.Rect;
using ERect = ElmSharp.Rect;
using PlatformView = ElmSharp.EvasObject;

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

			// TODO. It is workaroud code, Controls.VisualElement.MeasureOverride implementation is wrong. it does not apply Height/WidthRequest
			return VisualElementRenderer.Element.Measure(widthConstraint, heightConstraint).Request;
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

		public override ERect GetPlatformContentGeometry()
		{
			return VisualElementRenderer?.GetNativeContentGeometry() ?? new ERect();
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
