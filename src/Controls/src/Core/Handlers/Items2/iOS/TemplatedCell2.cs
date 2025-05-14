#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public class TemplatedCell2 : ItemsViewCell2, IPlatformMeasureInvalidationController
	{
		internal const string ReuseId = "Microsoft.Maui.Controls.TemplatedCell2";

		readonly WeakEventManager _weakEventManager = new();

		public event EventHandler<EventArgs> ContentSizeChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		public event EventHandler<LayoutAttributesChangedEventArgs2> LayoutAttributesChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		protected CGSize ConstrainedSize;

		protected nfloat ConstrainedDimension;

		WeakReference<DataTemplate> _currentTemplate;

		bool _bound;
		bool _measureInvalidated;
		bool _needsArrange;
		Size _measuredSize;
		Size _cachedConstraints;

		internal bool MeasureInvalidated => _measureInvalidated;

		// Flags changes confined to the header/footer, preventing unnecessary recycling and revalidation of templated cells.
		internal bool isHeaderOrFooterChanged = false;

		public DataTemplate CurrentTemplate
		{
			get => _currentTemplate is not null && _currentTemplate.TryGetTarget(out var target) ? target : null;
			private set => _currentTemplate = value is null ? null : new(value);
		}

		// Keep track of the cell size so we can verify whether a measure invalidation 
		// actually changed the size of the cell
		//Size _size;

		//internal CGSize CurrentSize => _size.ToCGSize();

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public TemplatedCell2(CGRect frame) : base(frame)
		{

		}

		public UICollectionViewScrollDirection ScrollDirection { get; set; }

		internal IPlatformViewHandler PlatformHandler { get; set; }

		internal UIView PlatformView { get; set; }

		internal void Unbind()
		{
			_bound = false;

			if (PlatformHandler?.VirtualView is View view)
			{
				//view.MeasureInvalidated -= MeasureInvalidated;
				view.BindingContext = null;
				(view.Parent as ItemsView)?.RemoveLogicalChild(view);
			}
		}

		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(
			UICollectionViewLayoutAttributes layoutAttributes)
		{
			var preferredAttributes = base.PreferredLayoutAttributesFittingAttributes(layoutAttributes);

			if (PlatformHandler?.VirtualView is { } virtualView)
			{
				var constraints = GetMeasureConstraints(preferredAttributes);

				if (_measureInvalidated || _cachedConstraints != constraints)
				{
					if (PlatformHandler?.VirtualView is View view && view.Parent is CollectionView itemsView && itemsView.ItemSizingStrategy == ItemSizingStrategy.MeasureFirstItem)
					{
						_measuredSize = view.Measure(preferredAttributes.Size.Width, preferredAttributes.Size.Height);
					}
					else
					{
						_measuredSize = virtualView.Measure(constraints.Width, constraints.Height);

					}
					_cachedConstraints = constraints;
					_needsArrange = true;
				}

				var preferredSize = preferredAttributes.Size;
				// Use measured size only when unconstrained
				var size = new Size(
					double.IsPositiveInfinity(constraints.Width) ? _measuredSize.Width : preferredSize.Width,
					double.IsPositiveInfinity(constraints.Height) ? _measuredSize.Height : preferredSize.Height
				);

				preferredAttributes.Frame = new CGRect(preferredAttributes.Frame.Location, size);
				preferredAttributes.ZIndex = 2;

				_measureInvalidated = false;
			}

			return preferredAttributes;
		}

		private protected virtual Size GetMeasureConstraints(UICollectionViewLayoutAttributes preferredAttributes)
		{
			var constraints = ScrollDirection == UICollectionViewScrollDirection.Vertical
				? new Size(preferredAttributes.Size.Width, double.PositiveInfinity)
				: new Size(double.PositiveInfinity, preferredAttributes.Size.Height);
			return constraints;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();

			if (PlatformHandler?.VirtualView is { } virtualView)
			{
				var boundsSize = Bounds.Size.ToSize();
				if (!_needsArrange)
				{
					// While rotating the device, and under other circumstances,
					// a layout pass is being triggered without going through PreferredLayoutAttributesFittingAttributes first.
					// In this case we should not trigger an Arrange pass because
					// the last measurement does not match the new bounds size.
					return;
				}

				_needsArrange = false;

				// We now have to apply the new bounds size to the virtual view
				// which will automatically set the frame on the platform view too.
				var frame = new Rect(Point.Zero, boundsSize);
				virtualView.Arrange(frame);
			}
		}

		public override void PrepareForReuse()
		{
			//Unbind();
			base.PrepareForReuse();
		}

		public void Bind(DataTemplate template, object bindingContext, ItemsView itemsView)
		{
			View virtualView = null;
			if (CurrentTemplate != template)
			{
				CurrentTemplate = template;
				virtualView = template.CreateContent(bindingContext, itemsView) as View;
			}
			else if (PlatformHandler?.VirtualView is View existingView)
			{
				virtualView = existingView;
			}

			BindVirtualView(virtualView, bindingContext, itemsView, false);
		}

		public void Bind(View virtualView, ItemsView itemsView)
		{
			BindVirtualView(virtualView, itemsView.BindingContext, itemsView, true);
		}

		void BindVirtualView(View virtualView, object bindingContext, ItemsView itemsView, bool needsContainer)
		{
			var oldElement = PlatformHandler?.VirtualView as View;

			if (oldElement is not null && oldElement != virtualView && isHeaderOrFooterChanged)
			{
				oldElement.BindingContext = null;
				itemsView.RemoveLogicalChild(oldElement);
				PlatformHandler = null;
				PlatformView?.RemoveFromSuperview();
			}

			if (PlatformHandler is null && virtualView is not null)
			{
				var mauiContext = itemsView.FindMauiContext()!;
				var nativeView = virtualView.ToPlatform(mauiContext);

				if (needsContainer)
				{
					PlatformView = new GeneralWrapperView(virtualView, mauiContext);
				}
				else
				{
					PlatformView = nativeView;
				}

				PlatformHandler = virtualView.Handler as IPlatformViewHandler;
				SetupPlatformView(PlatformView, needsContainer);
				ContentView.MarkAsCrossPlatformLayoutBacking();

				virtualView.BindingContext = bindingContext;
				itemsView.AddLogicalChild(virtualView);
			}

			if (PlatformHandler?.VirtualView is View view)
			{
				view.SetValueFromRenderer(BindableObject.BindingContextProperty, bindingContext);
				if (view.Parent is null)
				{
					itemsView.AddLogicalChild(view);
				}
			}

			_bound = true;
			((IPlatformMeasureInvalidationController)this).InvalidateMeasure();
			this.UpdateAccessibilityTraits(itemsView);
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

		//protected abstract (bool, Size) NeedsContentSizeUpdate(Size currentSize);

		// void MeasureInvalidated(object sender, EventArgs args)
		// {
		// 	var (needsUpdate, toSize) = NeedsContentSizeUpdate(_size);
		//
		// 	if (!needsUpdate)
		// 	{
		// 		return;
		// 	}
		//
		// 	// Cache the size for next time
		// 	_size = toSize;
		//
		// 	// Let the controller know that things need to be laid out again
		// 	OnContentSizeChanged();
		// }

		protected void OnContentSizeChanged()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(ContentSizeChanged));
		}

		protected void OnLayoutAttributesChanged(UICollectionViewLayoutAttributes newAttributes)
		{
			_weakEventManager.HandleEvent(this, new LayoutAttributesChangedEventArgs2(newAttributes), nameof(LayoutAttributesChanged));
		}

		//protected abstract bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes);

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

		void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
		{
			// This is a no-op
		}

		bool IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
		{
			// If the cell is not bound (or getting unbounded), we don't want to measure it
			// and cause a useless and harming InvalidateLayout on the collection view layout
			if (!_measureInvalidated && _bound)
			{
				_measureInvalidated = true;
				return true;
			}

			return false;
		}
	}
}
