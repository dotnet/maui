using ElmSharp;
using ELayout = ElmSharp.Layout;

namespace Xamarin.Forms.Platform.Tizen
{
	public sealed class DefaultRenderer : VisualElementRenderer<VisualElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<VisualElement> e)
		{
			if (NativeView == null)
			{
				var control = new ELayout(Forms.NativeParent);
				SetNativeView(control);
			}
			base.OnElementChanged(e);
		}
	}
}