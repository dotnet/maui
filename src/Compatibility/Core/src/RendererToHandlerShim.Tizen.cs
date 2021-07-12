using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Microsoft.Maui.Graphics;
using ERect = ElmSharp.Rect;
using NativeView = ElmSharp.EvasObject;

namespace Microsoft.Maui.Controls.Compatibility
{
	public partial class RendererToHandlerShim : INativeViewHandler
	{
		protected override NativeView CreateNativeView()
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
				NativeArrange(VisualElementRenderer.Element.Bounds);
			}
		}

		public override ERect GetNativeContentGeometry()
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
