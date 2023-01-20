using Microsoft.Maui.Controls.Platform;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete]
	public class DefaultRenderer : VisualElementRenderer<VisualElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<VisualElement> e)
		{
			if (NativeView == null)
			{
				var control = new NView();
				SetNativeView(control);
			}
			base.OnElementChanged(e);
		}
	}

#pragma warning disable CS0612 // Type or member is obsolete
	public class NativeViewWrapperRenderer : DefaultRenderer { }
#pragma warning restore CS0612 // Type or member is obsolete
}
