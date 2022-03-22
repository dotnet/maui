using ElmSharp;
using Microsoft.Maui.Controls.Platform;
using ESize = ElmSharp.Size;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
#pragma warning disable CS0618 // Type or member is obsolete
	public class NativeViewWrapperRenderer : ViewRenderer<NativeViewWrapper, EvasObject>
#pragma warning disable CS0618 // Type or member is obsolete
	{
		protected override void OnElementChanged(ElementChangedEventArgs<NativeViewWrapper> e)
		{
			if (Control == null)
				SetNativeControl(Element.EvasObject);

			base.OnElementChanged(e);
		}

		protected override ESize Measure(int availableWidth, int availableHeight)
		{
			if (Element?.MeasureDelegate == null)
			{
				return base.Measure(availableWidth, availableHeight);
			}

			// The user has specified a different implementation of MeasureDelegate
			ESize? result = Element.MeasureDelegate(this, availableWidth, availableHeight);

			// If the delegate returns a ElmSharp.Size, we use it; if it returns null,
			// fall back to the default implementation
			return result ?? base.Measure(availableWidth, availableHeight);
		}
	}
}
