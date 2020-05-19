using ElmSharp;
using ELayout = ElmSharp.Layout;

namespace System.Maui.Platform.Tizen
{
	public sealed class DefaultRenderer : VisualElementRenderer<VisualElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<VisualElement> e)
		{
			if (NativeView == null)
			{
				var control = new ELayout(System.Maui.Maui.NativeParent);
				SetNativeView(control);
			}
			base.OnElementChanged(e);
		}
	}
}