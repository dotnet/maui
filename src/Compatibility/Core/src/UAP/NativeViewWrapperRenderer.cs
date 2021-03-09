using Microsoft.UI.Xaml;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class NativeViewWrapperRenderer : ViewRenderer<NativeViewWrapper, FrameworkElement>
	{
		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Element?.GetDesiredSizeDelegate == null)
			{
				return base.GetDesiredSize(widthConstraint, heightConstraint);
			}

			// The user has specified a different implementation of GetDesiredSize
			SizeRequest? result = Element.GetDesiredSizeDelegate(this, widthConstraint, heightConstraint);

			// If the delegate returns a SizeRequest, we use it; 
			// if it returns null, fall back to the default implementation
			return result ?? base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		protected override Windows.Foundation.Size ArrangeOverride(Windows.Foundation.Size finalSize)
		{
			if (Element?.ArrangeOverrideDelegate == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			// The user has specified a different implementation of ArrangeOverride
			Windows.Foundation.Size? result = Element.ArrangeOverrideDelegate(this, finalSize);

			// If the delegate returns a Size, we use it; 
			// if it returns null, fall back to the default implementation
			return result ?? base.ArrangeOverride(finalSize);
		}

		protected Windows.Foundation.Size MeasureOverride()
		{
			return MeasureOverride(new Windows.Foundation.Size());
		}

		protected override Windows.Foundation.Size MeasureOverride(Windows.Foundation.Size availableSize)
		{
			if (Element?.MeasureOverrideDelegate == null)
			{
				return base.MeasureOverride(availableSize);
			}

			// The user has specified a different implementation of MeasureOverride
			Windows.Foundation.Size? result = Element.MeasureOverrideDelegate(this, availableSize);

			// If the delegate returns a Size, we use it; 
			// if it returns null, fall back to the default implementation
			return result ?? base.MeasureOverride(availableSize);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<NativeViewWrapper> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				SetNativeControl(Element.NativeElement);
				Control.SizeChanged += (sender, args) => { ((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged); };
			}
		}
	}
}