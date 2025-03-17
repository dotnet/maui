using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
#pragma warning disable CS0618 // Type or member is obsolete
	public partial class NativeViewWrapperRenderer : ViewRenderer<NativeViewWrapper, FrameworkElement>
#pragma warning restore CS0618 // Type or member is obsolete
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

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (Element?.ArrangeOverrideDelegate == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			// The user has specified a different implementation of ArrangeOverride
			global::Windows.Foundation.Size? result = Element.ArrangeOverrideDelegate(this, finalSize);

			// If the delegate returns a Size, we use it; 
			// if it returns null, fall back to the default implementation
			return result ?? base.ArrangeOverride(finalSize);
		}

		protected global::Windows.Foundation.Size MeasureOverride()
		{
			return MeasureOverride(new global::Windows.Foundation.Size());
		}

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (Element?.MeasureOverrideDelegate == null)
			{
				return base.MeasureOverride(availableSize);
			}

			// The user has specified a different implementation of MeasureOverride
			global::Windows.Foundation.Size? result = Element.MeasureOverrideDelegate(this, availableSize);

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