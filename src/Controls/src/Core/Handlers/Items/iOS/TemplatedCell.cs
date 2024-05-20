#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract class TemplatedCell : ItemsViewCell
	{
		public event EventHandler<EventArgs> ContentSizeChanged;
		public event EventHandler<LayoutAttributesChangedEventArgs> LayoutAttributesChanged;

		protected CGSize ConstrainedSize;

		protected nfloat ConstrainedDimension;

		private WeakReference<DataTemplate> _currentTemplate;

		public DataTemplate CurrentTemplate
		{
			get => _currentTemplate is not null && _currentTemplate.TryGetTarget(out var target) ? target : null;
			private set => _currentTemplate = value is null ? null : new(value);
		}

		// Keep track of the cell size so we can verify whether a measure invalidation 
		// actually changed the size of the cell
		Size _size;

		internal CGSize CurrentSize => _size.ToCGSize();

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		protected TemplatedCell(CGRect frame) : base(frame)
		{
		}

		WeakReference<IPlatformViewHandler> _handler;

		internal IPlatformViewHandler PlatformHandler
		{
			get => _handler is not null && _handler.TryGetTarget(out var h) ? h : null;
			set => _handler = value == null ? null : new(value);
		}

		public override void ConstrainTo(CGSize constraint)
		{
			ClearConstraints();
			ConstrainedSize = constraint;
		}

		public override void ConstrainTo(nfloat constant)
		{
			ClearConstraints();
			ConstrainedDimension = constant;
		}

		protected void ClearConstraints()
		{
			ConstrainedSize = default;
			ConstrainedDimension = default;
		}

		internal void Unbind()
		{
			if (PlatformHandler?.VirtualView is View view)
			{
				view.MeasureInvalidated -= MeasureInvalidated;
				view.BindingContext = null;
			}
		}

		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(
			UICollectionViewLayoutAttributes layoutAttributes)
		{
			var preferredAttributes = base.PreferredLayoutAttributesFittingAttributes(layoutAttributes);

			var preferredSize = preferredAttributes.Frame.Size;

			if (preferredSize.IsCloseTo(_size)
				&& AttributesConsistentWithConstrainedDimension(preferredAttributes))
			{
				return preferredAttributes;
			}

			var size = UpdateCellSize();

			// Adjust the preferred attributes to include space for the Forms element
			preferredAttributes.Frame = new CGRect(preferredAttributes.Frame.Location, size);

			OnLayoutAttributesChanged(preferredAttributes);

			return preferredAttributes;
		}

		CGSize UpdateCellSize()
		{
			// Measure this cell (including the Forms element) if there is no constrained size
			var size = ConstrainedSize == default ? Measure() : ConstrainedSize;

			// Update the size of the root view to accommodate the Forms element
			var platformView = PlatformHandler.ToPlatform();
			platformView.Frame = new CGRect(CGPoint.Empty, size);

			// Layout the Maui element 
			var nativeBounds = platformView.Frame.ToRectangle();
			PlatformHandler.VirtualView.Arrange(nativeBounds);
			_size = nativeBounds.Size;

			return size;
		}

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected void Layout(CGSize constraints)
		{
			var platformView = PlatformHandler.ToPlatform();

			var width = constraints.Width;
			var height = constraints.Height;

			PlatformHandler.VirtualView.Measure(width, height);

			platformView.Frame = new CGRect(0, 0, width, height);

			var rectangle = platformView.Frame.ToRectangle();
			PlatformHandler.VirtualView.Arrange(rectangle);
			_size = rectangle.Size;
		}

		public override void PrepareForReuse()
		{
			Unbind();
			base.PrepareForReuse();
		}

		public void Bind(DataTemplate template, object bindingContext, ItemsView itemsView)
		{
			var oldElement = PlatformHandler?.VirtualView as View;

			// Run this through the extension method in case it's really a DataTemplateSelector
			var itemTemplate = template.SelectDataTemplate(bindingContext, itemsView);

			if (itemTemplate != CurrentTemplate)
			{
				// Remove the old view, if it exists
				if (oldElement != null)
				{
					oldElement.MeasureInvalidated -= MeasureInvalidated;
					oldElement.BindingContext = null;
					itemsView.RemoveLogicalChild(oldElement);
					ClearSubviews();
					_size = Size.Zero;
				}

				// Create the content and renderer for the view 
				var view = itemTemplate.CreateContent() as View;

				// Set the binding context _before_ we create the renderer; that way, it's available during OnElementChanged
				view.BindingContext = bindingContext;

				var renderer = TemplateHelpers.GetHandler(view, itemsView.FindMauiContext());
				SetRenderer(renderer);

				// And make the new Element a "child" of the ItemsView
				// We deliberately do this _after_ setting the binding context for the new element;
				// if we do it before, the element briefly inherits the ItemsView's bindingcontext and we 
				// emit a bunch of needless binding errors
				itemsView.AddLogicalChild(view);

				UpdateSelectionColor(view);
			}
			else
			{
				// Same template
				if (oldElement != null)
				{
					oldElement.BindingContext = bindingContext;
					oldElement.MeasureInvalidated += MeasureInvalidated;

					UpdateCellSize();
				}
			}

			CurrentTemplate = itemTemplate;
		}

		void SetRenderer(IPlatformViewHandler renderer)
		{
			PlatformHandler = renderer;

			var platformView = PlatformHandler.ToPlatform();

			// Clear out any old views if this cell is being reused
			ClearSubviews();

			InitializeContentConstraints(platformView);

			UpdateVisualStates();

			(renderer.VirtualView as View).MeasureInvalidated += MeasureInvalidated;
		}

		void ClearSubviews()
		{
			for (int n = ContentView.Subviews.Length - 1; n >= 0; n--)
			{
				ContentView.Subviews[n].RemoveFromSuperview();
			}
		}

		internal void UseContent(TemplatedCell measurementCell)
		{
			// Copy all the content and values from the measurement cell 
			ConstrainedDimension = measurementCell.ConstrainedDimension;
			ConstrainedSize = measurementCell.ConstrainedSize;
			CurrentTemplate = measurementCell.CurrentTemplate;
			_size = measurementCell._size;
			SetRenderer(measurementCell.PlatformHandler);
		}

		bool IsUsingVSMForSelectionColor(View view)
		{
			var groups = VisualStateManager.GetVisualStateGroups(view);
			for (var groupIndex = 0; groupIndex < groups.Count; groupIndex++)
			{
				var group = groups[groupIndex];
				for (var stateIndex = 0; stateIndex < group.States.Count; stateIndex++)
				{
					var state = group.States[stateIndex];
					if (state.Name != VisualStateManager.CommonStates.Selected)
					{
						continue;
					}

					for (var setterIndex = 0; setterIndex < state.Setters.Count; setterIndex++)
					{
						var setter = state.Setters[setterIndex];
						if (setter.Property.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public override bool Selected
		{
			get => base.Selected;
			set
			{
				base.Selected = value;

				UpdateVisualStates();

				if (base.Selected)
				{
					// This must be called here otherwise the first item will have a gray background
					UpdateSelectionColor();
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

		protected void OnLayoutAttributesChanged(UICollectionViewLayoutAttributes newAttributes)
		{
			LayoutAttributesChanged?.Invoke(this, new LayoutAttributesChangedEventArgs(newAttributes));
		}

		protected abstract bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes);

		void UpdateVisualStates()
		{
			if (PlatformHandler?.VirtualView is VisualElement element)
			{
				VisualStateManager.GoToState(element, Selected
					? VisualStateManager.CommonStates.Selected
					: VisualStateManager.CommonStates.Normal);
			}
		}

		void UpdateSelectionColor()
		{
			if (PlatformHandler?.VirtualView is not View view)
			{
				return;
			}

			UpdateSelectionColor(view);
		}

		void UpdateSelectionColor(View view)
		{
			if (SelectedBackgroundView is null)
			{
				return;
			}

			// Prevents the use of default color when there are VisualStateManager with Selected state setting the background color
			// First we check whether the cell has the default selected background color; if it does, then we should check
			// to see if the cell content is the VSM to set a selected color

			if (ColorExtensions.AreEqual(SelectedBackgroundView.BackgroundColor, ColorExtensions.Gray) && IsUsingVSMForSelectionColor(view))
			{
				SelectedBackgroundView.BackgroundColor = UIColor.Clear;
			}
		}
	}
}
