using System.Windows;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class NativeViewWrapperRenderer : ViewRenderer<NativeViewWrapper, FrameworkElement>
	{
		public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (Element?.GetDesiredSizeDelegate == null)
				return base.GetDesiredSize(widthConstraint, heightConstraint);

			// The user has specified a different implementation of GetDesiredSize
			SizeRequest? result = Element.GetDesiredSizeDelegate(this, widthConstraint, heightConstraint);

			// If the delegate returns a SizeRequest, we use it; 
			// if it returns null, fall back to the default implementation
			return result ?? base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			if (Element?.ArrangeOverrideDelegate == null)
				return base.ArrangeOverride(finalSize);

			// The user has specified a different implementation of ArrangeOverride
			System.Windows.Size? result = Element.ArrangeOverrideDelegate(this, finalSize);

			// If the delegate returns a Size, we use it; 
			// if it returns null, fall back to the default implementation
			return result ?? base.ArrangeOverride(finalSize);
		}

		protected System.Windows.Size MeasureOverride()
		{
			return MeasureOverride(new System.Windows.Size());
		}

		protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
		{
			if (Element?.MeasureOverrideDelegate == null)
				return base.MeasureOverride(availableSize);

			// The user has specified a different implementation of MeasureOverride
			System.Windows.Size? result = Element.MeasureOverrideDelegate(this, availableSize);

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
				Control.LayoutUpdated += (sender, args) => { ((IVisualElementController)Element)?.InvalidateMeasure(InvalidationTrigger.MeasureChanged); };
			}
		}
	}
}