#nullable disable
using System;
using System.ComponentModel;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract class TemplatedCell : ItemsViewCell, IPlatformMeasureInvalidationController
	{
		readonly WeakEventManager _weakEventManager = new();

		public event EventHandler<EventArgs> ContentSizeChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		public event EventHandler<LayoutAttributesChangedEventArgs> LayoutAttributesChanged
		{
			add => _weakEventManager.AddEventHandler(value);
			remove => _weakEventManager.RemoveEventHandler(value);
		}

		protected CGSize ConstrainedSize;

		protected nfloat ConstrainedDimension;

		WeakReference<DataTemplate> _currentTemplate;

		public DataTemplate CurrentTemplate
		{
			get => _currentTemplate is not null && _currentTemplate.TryGetTarget(out var target) ? target : null;
			private set => _currentTemplate = value is null ? null : new(value);
		}

		// Keep track of the cell size so we can verify whether a measure invalidation 
		// actually changed the size of the cell
		Size _size;
		bool _bound;

		internal CGSize CurrentSize => _size.ToCGSize();

		[Export("initWithFrame:")]
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		protected TemplatedCell(CGRect frame) : base(frame)
		{
		}

		WeakReference<IPlatformViewHandler> _handler;
		bool _measureInvalidated;
		bool _needsArrange;

		internal bool MeasureInvalidated => _measureInvalidated;

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
			_bound = false;

			// Transient cleanup only: clear the bound view's BindingContext and detach it from
			// the ItemsView's logical children so it doesn't stay permanently rooted if the cell
			// is discarded, but deliberately leave the native subtree, PlatformHandler, and
			// CurrentTemplate intact. CarouselView Loop mode (and
			// ItemsViewController.CellDisplayingEndedFromDelegate) may call this speculatively on
			// a cell about to be redisplayed with the same content, and Bind() relies on the
			// native view still being attached to cheaply reattach the logical child without
			// re-running SetupPlatformView() (which would otherwise install duplicate Auto Layout
			// constraints on every reuse cycle).
			ClearBindingContext();
			DetachFromItemsView();
		}

		// Clears the bound view's BindingContext. Called during routine recycling and final teardown.
		void ClearBindingContext()
		{
			if (PlatformHandler?.VirtualView is View view)
			{
				view.BindingContext = null;
			}
		}

		// Permanent teardown for cells/content that are discarded outright and never
		// reused/rebound: template replacement, deterministic disposal, and measurement-cell
		// eviction (ItemsViewController.ClearMeasurementCells). Unlike Unbind(), this also
		// clears the native subtree and disconnects the handler tree so the bound view and
		// its platform view become collectible.
		void ReleaseContent()
		{
			var view = PlatformHandler?.VirtualView as View;

			ClearBindingContext();
			DetachFromItemsView();
			ClearSubviews();

			view?.DisconnectHandlers();
			PlatformHandler = null;
			CurrentTemplate = null;
		}

		internal void UnbindAndDisconnect()
		{
			ReleaseContent();
		}

		// Removes the bound view from the ItemsView's logical children, so it (and its handler) can
		// be collected. Now also invoked from Unbind() for transient cleanup (previously reserved
		// only for final teardown paths via ItemsViewController.Disconnect()/ClearMeasurementCells())
		// as part of fixing the leak where a discarded/reused cell's bound view stayed permanently
		// rooted as a logical child of the ItemsView.
		internal void DetachFromItemsView()
		{
			if (PlatformHandler?.VirtualView is View view)
			{
				(view.Parent as ItemsView)?.RemoveLogicalChild(view);
			}
		}

		protected override void Dispose(bool disposing)
		{
			// Only run managed/native cleanup on the deterministic disposing path. When
			// disposing == false (finalizer), touching the managed object graph
			// (BindingContext, LogicalChildren, events) is unsafe: it can run on the
			// finalizer thread and reference objects that are themselves being finalized.
			// ReleaseContent() covers the case where the cell is deallocated without ever
			// going through PrepareForReuse/Unbind, permanently detaching and disconnecting
			// the bound view since the cell itself is being torn down for good.
			if (disposing)
			{
				ReleaseContent();
			}

			base.Dispose(disposing);
		}

		public override UICollectionViewLayoutAttributes PreferredLayoutAttributesFittingAttributes(
			UICollectionViewLayoutAttributes layoutAttributes)
		{
			var preferredAttributes = base.PreferredLayoutAttributesFittingAttributes(layoutAttributes);

			// Skip measuring cells that aren't currently bound (e.g. between Unbind() and a
			// pending Bind() during rapid section restructuring). Measure() unconditionally
			// dereferences PlatformHandler.VirtualView and would otherwise throw an NRE.
			if (!_bound || PlatformHandler?.VirtualView is null)
			{
				return preferredAttributes;
			}

			if (_measureInvalidated ||
				!AttributesConsistentWithConstrainedDimension(preferredAttributes) ||
				!preferredAttributes.Frame.Size.IsCloseTo(_size))
			{
				// Measure this cell (including the Forms element) if there is no constrained size
				var size = ConstrainedSize == default ? Measure() : ConstrainedSize;
				_size = size.ToSize();
				_needsArrange = true;
				_measureInvalidated = false;
				preferredAttributes.Frame = new CGRect(preferredAttributes.Frame.Location, _size);
				// Ensure we get a layout pass to arrange the virtual view.
				// This is not happening sometimes due to the way we update constraints on visible cells.
				SetNeedsLayout();
				OnLayoutAttributesChanged(preferredAttributes);
			}

			return preferredAttributes;
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

				// Inject per-cell safe area insets into the MauiView for CrossPlatformArrange
				// to apply as internal padding. UICollectionView bypasses MAUI's arrange chain,
				// so cells cannot use the standard safe area flow (#33604, #34635).
				MauiView.ApplyCellSafeAreaOverride(this, virtualView, PlatformHandler.ToPlatform());

				virtualView.Arrange(frame);
			}
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
			_measureInvalidated = false;
		}

		public override void PrepareForReuse()
		{
			_bound = false;
			base.PrepareForReuse();
		}

		public void Bind(DataTemplate template, object bindingContext, ItemsView itemsView)
		{
			var oldElement = PlatformHandler?.VirtualView as View;

			// Run this through the extension method in case it's really a DataTemplateSelector
			var itemTemplate = template.SelectDataTemplate(bindingContext, itemsView);

			if (itemTemplate != CurrentTemplate)
			{
				// Remove the old view, if it exists. A template change discards oldElement's
				// renderer for good, so this permanently detaches and disconnects it to avoid
				// leaking (unlike the same-template branch below, which keeps the native
				// subtree attached for cheap reuse). ReleaseContent() clears the BindingContext,
				// removes the logical child, clears native subviews, and disconnects the handler
				// tree (equivalent to the previous oldElement.DisconnectHandlers() call, since
				// view == oldElement here).
				if (oldElement != null)
				{
					ReleaseContent();
				}

				// Create the content and renderer for the view 
				var content = itemTemplate.CreateContent();

				if (content is not View view)
				{
					throw new InvalidOperationException($"{itemTemplate} could not be created from {content}");
				}

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
				if (oldElement != null && !ReferenceEquals(bindingContext, oldElement.BindingContext))
				{
					oldElement.BindingContext = bindingContext;
				}

				// This cell may have been unbound while off-screen (e.g. its previous item was
				// removed from the ItemsSource, see ItemsViewController.CellDisplayingEndedFromDelegate),
				// which detaches the view from the ItemsView's logical children (Parent becomes null).
				// If the cell is now being reused/rebound with the same template, the view must be
				// re-attached; otherwise it silently drops out of the logical tree even though it's
				// visibly bound and displayed again. Mirrors TemplatedCell2.BindVirtualView().
				//
				// Note: the native subtree itself never gets detached by Unbind() (only the
				// logical-child link does), so there's no need to reattach/recreate it here -
				// doing so would call SetupPlatformView() again and install duplicate Auto
				// Layout constraints on every reuse cycle.
				if (oldElement is not null && oldElement.Parent is null)
				{
					itemsView.AddLogicalChild(oldElement);
				}
			}

			CurrentTemplate = itemTemplate;
			this.UpdateAccessibilityTraits(itemsView);
			MarkAsBound();
		}

		void MarkAsBound()
		{
			_bound = true;
			this.InvalidateMeasure();
		}

		void SetRenderer(IPlatformViewHandler renderer)
		{
			PlatformHandler = renderer;

			var platformView = PlatformHandler.ToPlatform();

			// Clear out any old views if this cell is being reused
			ClearSubviews();

			SetupPlatformView(platformView);
			ContentView.MarkAsCrossPlatformLayoutBacking();

			UpdateVisualStates();
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
			_bound = true;
			((IPlatformMeasureInvalidationController)this).InvalidateMeasure();

			// Complete the ownership transfer on the donor so it no longer references the handler
			// it just gave away. Without this, if the donor cell is later disposed directly
			// (Dispose() is a public entry point on this unsealed type) it would tear down the
			// handler subtree that now belongs to this cell.
			measurementCell.PlatformHandler = null;
			measurementCell.CurrentTemplate = null;
			measurementCell._bound = false;
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
						if (setter.Property.PropertyName == VisualElement.BackgroundColorProperty.PropertyName ||
							setter.Property.PropertyName == VisualElement.BackgroundProperty.PropertyName)
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

		protected void OnContentSizeChanged()
		{
			_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(ContentSizeChanged));
		}

		protected void OnLayoutAttributesChanged(UICollectionViewLayoutAttributes newAttributes)
		{
			_weakEventManager.HandleEvent(this, new LayoutAttributesChangedEventArgs(newAttributes), nameof(LayoutAttributesChanged));
		}

		protected abstract bool AttributesConsistentWithConstrainedDimension(UICollectionViewLayoutAttributes attributes);

		void UpdateVisualStates()
		{
			if (PlatformHandler?.VirtualView is VisualElement element)
			{
				element.IsItemSelected = Selected;
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
			// This is a no-op for cells
		}
	}
}
