using ElmSharp;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen
{
	public class NativeViewWrapperRenderer : ViewRenderer<NativeViewWrapper, EvasObject>
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
