using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public abstract class TemplatedCell : ItemsViewCell
	{
		public event EventHandler<EventArgs> ContentSizeChanged;
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
			var preferredAttributes = base.PreferredLayoutAttributesFittingAttributes(layoutAttributes);

			// Measure this cell (including the Forms element)
			var size = Measure();

			// Update the size of the root view to accommodate the Forms element
			var nativeView = VisualElementRenderer.NativeView;
			nativeView.Frame = new CGRect(CGPoint.Empty, size);

			// Layout the Forms element 
			VisualElementRenderer.Element.Layout(nativeView.Frame.ToRectangle());

			// Adjust the preferred attributes to include space for the Forms element
			preferredAttributes.Frame = new CGRect(preferredAttributes.Frame.Location, size);

			return preferredAttributes;
		}

		public override void PrepareForReuse()
		{
			base.PrepareForReuse();
			ClearSubviews();
		}

		public void SetRenderer(IVisualElementRenderer renderer)
		{
			VisualElementRenderer = renderer;
			var nativeView = VisualElementRenderer.NativeView;

			InitializeContentConstraints(nativeView);

			renderer.Element.MeasureInvalidated += MeasureInvalidated;
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
				ContentView.Subviews[n].RemoveFromSuperview();
			}
		}

		public override bool Selected
		{
			get => base.Selected;
			set
			{
				base.Selected = value;

				var element = VisualElementRenderer?.Element;

				if (element != null)
				{
					VisualStateManager.GoToState(element, value
						? VisualStateManager.CommonStates.Selected
						: VisualStateManager.CommonStates.Normal);
				}
			}
		}

		void MeasureInvalidated(object sender, EventArgs args)
		{
			if (VisualElementRenderer?.Element == null)
			{
				return;
			}

			var bounds = VisualElementRenderer.Element.Bounds;

			if (bounds.Width <= 0 || bounds.Height <= 0)
			{
				return;
			}

			OnContentSizeChanged();
		}

		public void PrepareForRemoval()
		{
			if (VisualElementRenderer?.Element != null)
			{
				VisualElementRenderer.Element.MeasureInvalidated -= MeasureInvalidated;
			}
		}

		protected void OnContentSizeChanged()
		{
			ContentSizeChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}