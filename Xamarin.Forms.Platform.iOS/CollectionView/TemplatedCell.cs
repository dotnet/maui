using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	// TODO hartez 2018/09/17 14:11:02 Should this be named "TemplateCell" instead of "TemplatedCell"?	
	public abstract class TemplatedCell : ItemsViewCell
	{
		protected nfloat ConstrainedDimension;

		[Export("initWithFrame:")]
		protected TemplatedCell(CGRect frame) : base(frame)
		{
		}

		public IVisualElementRenderer VisualElementRenderer { get; private set; }

		public override void ConstrainTo(nfloat constant)
		{
			ConstrainedDimension = constant;
		}

		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(
			UICollectionViewLayoutAttributes layoutAttributes)
		{
			var nativeView = VisualElementRenderer.NativeView;

			var size = Measure();

			nativeView.Frame = new CGRect(CGPoint.Empty, size);
			VisualElementRenderer.Element.Layout(nativeView.Frame.ToRectangle());

			layoutAttributes.Frame = VisualElementRenderer.NativeView.Frame;

			return layoutAttributes;
		}

		public void SetRenderer(IVisualElementRenderer renderer)
		{
			ClearSubviews();

			VisualElementRenderer = renderer;
			var nativeView = VisualElementRenderer.NativeView;

			InitializeContentConstraints(nativeView);
		}

		protected void Layout(CGSize constraints)
		{
			var nativeView = VisualElementRenderer.NativeView;

			var width = constraints.Width;
			var height = constraints.Height;

			VisualElementRenderer.Element.Measure(width, height, MeasureFlags.IncludeMargins);

			nativeView.Frame = new CGRect(0, 0, width, height);

			VisualElementRenderer.Element.Layout(nativeView.Frame.ToRectangle());
		}

		void ClearSubviews()
		{
			for (int n = ContentView.Subviews.Length - 1; n >= 0; n--)
			{
				// TODO hartez 2018/09/07 16:14:43 Does this also need to clear the constraints?	
				ContentView.Subviews[n].RemoveFromSuperview();
			}
		}
	}
}