using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using NativeView = ElmSharp.EvasObject;
using ERect = ElmSharp.Rect;

namespace Microsoft.Maui.Controls.Compatibility
{
	public partial class RendererToHandlerShim
	{
		protected override NativeView CreateNativeView()
		{
			return VisualElementRenderer.NativeView;
		}

		IVisualElementRenderer CreateRenderer(IView view)
		{
			return Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view) ?? new DefaultRenderer();
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
	}
}
