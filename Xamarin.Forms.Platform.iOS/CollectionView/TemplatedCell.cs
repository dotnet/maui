using System;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	public abstract class TemplatedCell : ItemsViewCell
	{
		public event EventHandler<EventArgs> ContentSizeChanged;

		protected nfloat ConstrainedDimension;

		DataTemplate _currentTemplate;

		// Keep track of the cell size so we can verify whether a measure invalidation 
		// actually changed the size of the cell
		Size _size;

		[Export("initWithFrame:")]
		protected TemplatedCell(CGRect frame) : base(frame)
		{
		}

		internal IVisualElementRenderer VisualElementRenderer { get; private set; }

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
			var nativeBounds = nativeView.Frame.ToRectangle();
			VisualElementRenderer.Element.Layout(nativeBounds);
			_size = nativeBounds.Size;

			// Adjust the preferred attributes to include space for the Forms element
			preferredAttributes.Frame = new CGRect(preferredAttributes.Frame.Location, size);

			return preferredAttributes;
		}

		public void Bind(ItemsView itemsView, object bindingContext)
		{
			var template = itemsView.ItemTemplate;

			// Run this through the extension method in case it's really a DataTemplateSelector
			template = template.SelectDataTemplate(bindingContext, itemsView);

			Bind(template, bindingContext, itemsView);
		}

		public void Bind(DataTemplate template, object bindingContext, ItemsView itemsView)
		{
			var oldElement = VisualElementRenderer?.Element;

			if (template != _currentTemplate)
			{
				// Remove the old view, if it exists
				if (oldElement != null)
				{
					oldElement.MeasureInvalidated -= MeasureInvalidated;
					itemsView.RemoveLogicalChild(oldElement);
					ClearSubviews();
					_size = Size.Zero;
				}

				// Create the content and renderer for the view 
				var view = template.CreateContent() as View;
				var renderer = TemplateHelpers.CreateRenderer(view);
				SetRenderer(renderer);
			}

			var currentElement = VisualElementRenderer?.Element;

			// Bind the view to the data item
			currentElement.BindingContext = bindingContext;

			if (template != _currentTemplate)
			{
				// And make the Element a "child" of the ItemsView
				// We deliberately do this _after_ setting the binding context for the new element;
				// if we do it before, the element briefly inherits the ItemsView's bindingcontext and we 
				// emit a bunch of needless binding errors
				itemsView.AddLogicalChild(currentElement);
			}

			_currentTemplate = template;
		}

		void SetRenderer(IVisualElementRenderer renderer)
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

		protected abstract (bool, Size) NeedsContentSizeUpdate(Size currentSize);

		void MeasureInvalidated(object sender, EventArgs args)
		{
			var (needsUpdate, toSize) = NeedsContentSizeUpdate(_size);

			if (!needsUpdate)
			{
				return;
			}

			// Cache the size for next time
			_size = toSize;

			// Let the controller know that things need to be laid out again
			OnContentSizeChanged();
		}

		protected void OnContentSizeChanged()
		{
			ContentSizeChanged?.Invoke(this, EventArgs.Empty);
		}
	}
}