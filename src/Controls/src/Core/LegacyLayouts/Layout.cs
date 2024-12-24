#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls.Compatibility
{
	/// <summary>
	/// Base class for layouts that allow you to arrange and group UI controls in your application.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="View"/> that can be added to the layout.</typeparam>
	[ContentProperty(nameof(Children))]
	[Obsolete("Use Microsoft.Maui.Controls.Layout instead. For more information, see https://learn.microsoft.com/dotnet/maui/user-interface/layouts/custom")]
	public abstract partial class Layout<T> : Layout, Microsoft.Maui.ILayout, ILayoutManager, IBindableLayout, IViewContainer<T> where T : View
	{
		readonly ElementCollection<T> _children;

		protected Layout() => _children = new ElementCollection<T>(InternalChildren);

		/// <summary>
		/// Gets the child objects contained in this layout.
		/// </summary>
		public new IList<T> Children => _children;

		/// <summary>
		/// Gets the associated handler for this layout.
		/// </summary>
		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;

		IList IBindableLayout.Children => _children;

		bool ISafeAreaView.IgnoreSafeArea => false;

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			if (child is T typedChild)
			{
				OnAdded(typedChild);
			}
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);

			if (child is T typedChild)
			{
				OnRemoved(typedChild);
			}
		}

		/// <summary>
		/// Invoked when a child is added to the layout. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="view">The view which was added.</param>
		protected virtual void OnAdded(T view)
		{
		}

		/// <summary>
		/// Invoked when a child is removed the layout. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="view">The view which was removed.</param>
		protected virtual void OnRemoved(T view)
		{
		}

		Size ILayoutManager.Measure(double widthConstraint, double heightConstraint)
		{
			return OnMeasure(widthConstraint, heightConstraint);
		}

		Size ILayoutManager.ArrangeChildren(Rect bounds)
		{
			LayoutChildren(bounds.X, bounds.Y, bounds.Width, bounds.Height);
			return bounds.Size;
		}
	}

	/// <summary>
	/// Base class for layouts that allow you to arrange and group UI controls in your application.
	/// </summary>
	[Obsolete("Use Microsoft.Maui.Controls.Layout instead. For more information, see https://learn.microsoft.com/dotnet/maui/user-interface/layouts/custom")]
	public abstract class Layout : View, ILayout, ILayoutController, IPaddingElement, IView, IVisualTreeElement, IInputTransparentContainerElement
	{
		/// <summary>Bindable property for <see cref="IsClippedToBounds"/>.</summary>
		public static readonly BindableProperty IsClippedToBoundsProperty =
			BindableProperty.Create(nameof(IsClippedToBounds), typeof(bool), typeof(Layout), false,
				propertyChanged: IsClippedToBoundsPropertyChanged);

		/// <summary>Bindable property for <see cref="CascadeInputTransparent"/>.</summary>
		public static readonly BindableProperty CascadeInputTransparentProperty =
			BindableProperty.Create(nameof(CascadeInputTransparent), typeof(bool), typeof(Layout), true,
				propertyChanged: OnCascadeInputTransparentPropertyChanged);

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		bool _hasDoneLayout;
		Size _lastLayoutSize = new Size(-1, -1);

		protected Layout()
		{
			//if things were added in base ctor (through implicit styles), the items added aren't properly parented
			if (InternalChildren.Count > 0)
			{
				InternalChildrenOnCollectionChanged(this,
					new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, InternalChildren));
			}

			InternalChildren.CollectionChanged += InternalChildrenOnCollectionChanged;
		}

		/// <summary>
		/// Gets or sets a value which determines if the layout should clip its children to its bounds.
		/// The default value is <see langword="false"/>.
		/// </summary>
		public bool IsClippedToBounds
		{
			get => (bool)GetValue(IsClippedToBoundsProperty);
			set => SetValue(IsClippedToBoundsProperty, value);
		}

		/// <summary>
		/// Gets or sets the inner padding of the layout.
		/// The default value is a <see cref="Thickness"/> with all values set to 0.
		/// </summary>
		/// <remarks>The padding is the space between the bounds of a layout and the bounding region into which its children should be arranged into.</remarks>
		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

		/// <summary>
		/// Gets or sets a value that controls whether child elements
		/// inherit the input transparency of this layout when the tranparency is <see langword="true"/>.
		/// </summary>
		/// <value>
		/// <see langword="true" /> to cause child elements to inherit the input transparency of this layout,
		/// when this layout's <see cref="VisualElement.InputTransparent" /> property is <see langword="true" />.
		/// <see langword="false" /> to cause child elements to ignore the input tranparency of this layout.
		/// </value>
		public bool CascadeInputTransparent
		{
			get => (bool)GetValue(CascadeInputTransparentProperty);
			set => SetValue(CascadeInputTransparentProperty, value);
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => default(Thickness);

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue) => InvalidateLayout();

		static void IsClippedToBoundsPropertyChanged(BindableObject bindableObject, object oldValue, object newValue)
		{
			if (bindableObject is IView view)
			{
				view.Handler?.UpdateValue(nameof(Maui.ILayout.ClipsToBounds));
			}
		}

		private protected override IList<Element> LogicalChildrenInternalBackingStore
			=> InternalChildren;

		internal ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		/// <summary>
		/// Occurs at the end of a layout cycle if any of the child element's <see cref="VisualElement.Bounds" /> have changed.
		/// </summary>
		[Obsolete("Use SizeChanged.")]
		public event EventHandler LayoutChanged;

		/// <summary>The children contained in this layout.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public IReadOnlyList<Element> Children => InternalChildren;

		/// <summary>
		/// Forces a layout cycle on the element and all of its descendants.
		/// </summary>
		/// <remarks>Calling this method frequently can have negative impacts on performance.</remarks>
		[Obsolete("Call InvalidateMeasure instead depending on your scenario.")]
		public void ForceLayout() => SizeAllocated(Width, Height);

		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => Children.ToList().AsReadOnly();

#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable CS0618 // Type or member is obsolete
		public override SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
#pragma warning restore CS0618 // Type or member is obsolete
		{
#pragma warning disable CS0618 // Type or member is obsolete
			SizeRequest size = base.Measure(widthConstraint - Padding.HorizontalThickness, heightConstraint - Padding.VerticalThickness, flags);
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			return new SizeRequest(new Size(size.Request.Width + Padding.HorizontalThickness, size.Request.Height + Padding.VerticalThickness),
				new Size(size.Minimum.Width + Padding.HorizontalThickness, size.Minimum.Height + Padding.VerticalThickness));
#pragma warning restore CS0618 // Type or member is obsolete
		}
#pragma warning restore CS0672 // Member overrides obsolete member

		/// <summary>
		/// Positions a child element into a bounding region while respecting the child elements <see cref="View.HorizontalOptions" /> and <see cref="View.VerticalOptions" />.
		/// </summary>
		/// <param name="child">The child element to be positioned.</param>
		/// <param name="region">The bounding region in which the child should be positioned.</param>
		/// <remarks>This method is called in the layout cycle after the general regions for each child have been calculated.
		/// This method will handle positioning the element relative to the bounding region given if the bounding region given is larger than the child's desired size.</remarks>
		[Obsolete("Use the Arrange method on child instead.")]
		public static void LayoutChildIntoBoundingRegion(VisualElement child, Rect region)
		{
			bool isRightToLeft = false;
			if (child.Parent is IFlowDirectionController parent &&
				(isRightToLeft = parent.ApplyEffectiveFlowDirectionToChildContainer &&
				parent.EffectiveFlowDirection.IsRightToLeft()) &&
				(parent.Width - region.Right) != region.X)
			{
				region = new Rect(parent.Width - region.Right, region.Y, region.Width, region.Height);
			}

			if (child is IView fe && fe.Handler != null)
			{
				// The new arrange methods will take care of all the alignment and margins and such
				fe.Arrange(region);
				return;
			}

			if (!(child is View view))
			{
				child.Layout(region);
				return;
			}

			LayoutOptions horizontalOptions = view.HorizontalOptions;
			if (horizontalOptions.Alignment != LayoutAlignment.Fill)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				SizeRequest request = child.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
#pragma warning restore CS0618 // Type or member is obsolete
				double diff = Math.Max(0, region.Width - request.Request.Width);
				double horizontalAlign = horizontalOptions.Alignment.ToDouble();
				if (isRightToLeft)
				{
					horizontalAlign = 1 - horizontalAlign;
				}

				region.X += (int)(diff * horizontalAlign);
				region.Width -= diff;
			}

			LayoutOptions verticalOptions = view.VerticalOptions;
			if (verticalOptions.Alignment != LayoutAlignment.Fill)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				SizeRequest request = child.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
#pragma warning restore CS0618 // Type or member is obsolete
				double diff = Math.Max(0, region.Height - request.Request.Height);
				region.Y += (int)(diff * verticalOptions.Alignment.ToDouble());
				region.Height -= diff;
			}

			Thickness margin = view.Margin;
			region.X += margin.Left;
			region.Width -= margin.HorizontalThickness;
			region.Y += margin.Top;
			region.Height -= margin.VerticalThickness;

			child.Layout(region);
		}

		/// <summary>
		/// Sends a child to the back of the visual stack.
		/// </summary>
		/// <param name="view">The view to lower in the visual stack.</param>
		/// <remarks>Children are internally stored in visual stack order.
		/// This means that raising or lowering a child also changes the order in which the children are enumerated.</remarks>
		[Obsolete("Use the ZIndex Property instead")]
		public void LowerChild(View view)
		{
			if (!InternalChildren.Contains(view) || InternalChildren.First() == view)
			{
				return;
			}

			InternalChildren.Move(InternalChildren.IndexOf(view), 0);
			OnChildrenReordered();
		}

		/// <summary>
		/// Sends a child to the front of the visual stack.
		/// </summary>
		/// <param name="view">The view to raise in the visual stack.</param>
		/// <remarks>Children are internally stored in visual stack order.
		/// This means that raising or lowering a child also changes the order in which the children are enumerated.</remarks>
		[Obsolete("Use the ZIndex Property instead")]
		public void RaiseChild(View view)
		{
			if (!InternalChildren.Contains(view) || InternalChildren.Last() == view)
			{
				return;
			}

			InternalChildren.Move(InternalChildren.IndexOf(view), InternalChildren.Count - 1);
			OnChildrenReordered();
		}

		/// <summary>
		/// Invalidates the current layout.
		/// </summary>
		/// <remarks>Calling this method will invalidate the measure and triggers a new layout cycle.</remarks>
		[Obsolete("Use InvalidateMeasure depending on your scenario")]
		protected virtual void InvalidateLayout()
		{
			_hasDoneLayout = false;
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			if (!_hasDoneLayout)
			{
				ForceLayout();
			}
		}

		/// <summary>
		/// Positions and sizes the children of a layout.
		/// </summary>
		/// <param name="x">A value representing the x coordinate of the child region bounding box.</param>
		/// <param name="y">A value representing the y coordinate of the child region bounding box.</param>
		/// <param name="width">A value representing the width of the child region bounding box.</param>
		/// <param name="height">A value representing the height of the child region bounding box.</param>
		/// <remarks>Implementors wishing to change the default behavior of a Layout should override this method.
		/// It is suggested to still call the base method and modify its calculated results.</remarks>

		[Obsolete("Use ArrangeOverride")]
		protected abstract void LayoutChildren(double x, double y, double width, double height);

		internal override void OnChildMeasureInvalidatedInternal(VisualElement child, InvalidationTrigger trigger, int depth)
		{
			// TODO: once we remove old Xamarin public signatures we can invoke `OnChildMeasureInvalidated(VisualElement, InvalidationTrigger)` directly
			OnChildMeasureInvalidated(child, new InvalidationEventArgs(trigger, depth));
		}

		/// <summary>
		/// Invoked whenever a child of the layout has emitted <see cref="VisualElement.MeasureInvalidated" />.
		/// Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="sender">The child element whose preferred size changed.</param>
		/// <param name="e">The event data.</param>
		/// <remarks>This method has a default implementation and application developers must call the base implementation.</remarks>
		protected void OnChildMeasureInvalidated(object sender, EventArgs e)
		{
			var depth = 0;
			InvalidationTrigger trigger;
			if (e is InvalidationEventArgs args)
			{
				trigger = args.Trigger;
				depth = args.CurrentInvalidationDepth;
			}
			else
			{
				trigger = InvalidationTrigger.Undefined;
			}

			OnChildMeasureInvalidated((VisualElement)sender, trigger, depth);
			OnChildMeasureInvalidated();
		}

		/// <summary>
		/// Invoked whenever a child of the layout has emitted <see cref="VisualElement.MeasureInvalidated" />.
		/// Implement this method to add class handling for this event.
		/// </summary>
		[Obsolete("Subscribe to the MeasureInvalidated Event on the Children.")]
		protected virtual void OnChildMeasureInvalidated()
		{
		}

		Size IView.Measure(double widthConstraint, double heightConstraint)
		{
			DesiredSize = MeasureOverride(widthConstraint, heightConstraint);
			return DesiredSize;
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var sansMargins = OnMeasure(widthConstraint, heightConstraint).Request;
#pragma warning restore CS0618 // Type or member is obsolete
			return new Size(sansMargins.Width + Margin.HorizontalThickness, sansMargins.Height + Margin.VerticalThickness);
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			UpdateChildrenLayout();
		}

		/// <summary>
		/// When implemented, should return <see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" /> when added,
		/// and should return <see langword="false" /> if it should not call <see cref="VisualElement.InvalidateMeasure" />. The default value is <see langword="true" />.
		/// </summary>
		/// <param name="child">The child for which to specify whether or not to track invalidation.</param>
		/// <returns><see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" />, otherwise <see langword="false"/>.</returns>
		[Obsolete("If you want to influence invalidation override InvalidateMeasureOverride")]
		protected virtual bool ShouldInvalidateOnChildAdded(View child) => true;

		/// <summary>
		/// When implemented, should return <see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" /> when removed,
		/// and should return <see langword="false" /> if it should not call <see cref="VisualElement.InvalidateMeasure" />. The default value is <see langword="true" />.
		/// </summary>
		/// <param name="child">The child for which to specify whether or not to track invalidation.</param>
		/// <returns><see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" />, otherwise <see langword="false"/>.</returns>
		[Obsolete("If you want to influence invalidation override InvalidateMeasureOverride")]
		protected virtual bool ShouldInvalidateOnChildRemoved(View child) => true;

		/// <summary>
		/// Instructs the layout to relayout all of its children.
		/// </summary>
		/// <remarks>This method starts a new layout cycle for the layout. Invoking this method frequently can negatively impact performance.</remarks>
		[Obsolete("Use InvalidateMeasure depending on your scenario")]
		protected void UpdateChildrenLayout()
		{
			_hasDoneLayout = true;

			if (!ShouldLayoutChildren())
			{
				return;
			}

			var oldBounds = new Rect[LogicalChildrenInternal.Count];
			for (var index = 0; index < oldBounds.Length; index++)
			{
				if (LogicalChildrenInternal[index] is VisualElement c)
				{
					oldBounds[index] = c.Bounds;
				}
				else
				{
					// The Logical Children Of the Layout aren't VisualElements
					// This means layout won't automatically be performed by this code
					// This is really only relevant for controls that still inherit from the 
					// legacy layouts. I think SwipeView is the only control that runs into this
					// Because the children of SwipeView are all logical elements handled by the handler
					// not by this code.
					return;
				}
			}

			double width = Width;
			double height = Height;

			double x = Padding.Left;
			double y = Padding.Top;
			double w = Math.Max(0, width - Padding.HorizontalThickness);
			double h = Math.Max(0, height - Padding.VerticalThickness);

			var isHeadless = CompressedLayout.GetIsHeadless(this);
			var headlessOffset = CompressedLayout.GetHeadlessOffset(this);
			for (var i = 0; i < LogicalChildrenInternal.Count; i++)
			{
				CompressedLayout.SetHeadlessOffset((VisualElement)LogicalChildrenInternal[i], isHeadless ? new Point(headlessOffset.X + Bounds.X, headlessOffset.Y + Bounds.Y) : new Point());
			}

			_lastLayoutSize = new Size(width, height);

			LayoutChildren(x, y, w, h);

			for (var i = 0; i < oldBounds.Length; i++)
			{
				Rect oldBound = oldBounds[i];
				Rect newBound = ((VisualElement)LogicalChildrenInternal[i]).Bounds;
				if (oldBound != newBound)
				{
					LayoutChanged?.Invoke(this, EventArgs.Empty);
					return;
				}
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		internal static void LayoutChildIntoBoundingRegion(View child, Rect region, SizeRequest childSizeRequest)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			bool isRightToLeft = false;
			if (child.Parent is IFlowDirectionController parent && (isRightToLeft = parent.ApplyEffectiveFlowDirectionToChildContainer && parent.EffectiveFlowDirection.IsRightToLeft()))
			{
				region = new Rect(parent.Width - region.Right, region.Y, region.Width, region.Height);
			}

			if (child is IView fe && fe.Handler != null)
			{
				// The new arrange methods will take care of all the alignment and margins and such
				fe.Arrange(region);
				return;
			}

			if (region.Size != childSizeRequest.Request)
			{
				bool canUseAlreadyDoneRequest = region.Width >= childSizeRequest.Request.Width && region.Height >= childSizeRequest.Request.Height;

				LayoutOptions horizontalOptions = child.HorizontalOptions;
				if (horizontalOptions.Alignment != LayoutAlignment.Fill)
				{
#pragma warning disable CS0618 // Type or member is obsolete
					SizeRequest request = canUseAlreadyDoneRequest ? childSizeRequest : child.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
#pragma warning restore CS0618 // Type or member is obsolete
					double diff = Math.Max(0, region.Width - request.Request.Width);
					double horizontalAlign = horizontalOptions.Alignment.ToDouble();
					if (isRightToLeft)
					{
						horizontalAlign = 1 - horizontalAlign;
					}

					region.X += (int)(diff * horizontalAlign);
					region.Width -= diff;
				}

				LayoutOptions verticalOptions = child.VerticalOptions;
				if (verticalOptions.Alignment != LayoutAlignment.Fill)
				{
#pragma warning disable CS0618 // Type or member is obsolete
					SizeRequest request = canUseAlreadyDoneRequest ? childSizeRequest : child.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
#pragma warning restore CS0618 // Type or member is obsolete
					double diff = Math.Max(0, region.Height - request.Request.Height);
					region.Y += (int)(diff * verticalOptions.Alignment.ToDouble());
					region.Height -= diff;
				}
			}

			Thickness margin = child.Margin;
			region.X += margin.Left;
			region.Width -= margin.HorizontalThickness;
			region.Y += margin.Top;
			region.Height -= margin.VerticalThickness;

			child.Layout(region);
		}

		internal virtual void OnChildMeasureInvalidated(VisualElement child, InvalidationTrigger trigger, int depth)
		{
			IReadOnlyList<Element> children = LogicalChildrenInternal;
			int count = children.Count;
			for (var index = 0; index < count; index++)
			{
				if (LogicalChildrenInternal[index] is VisualElement v && v.IsVisible && (!v.IsPlatformEnabled || !v.IsPlatformStateConsistent))
				{
					return;
				}
			}

			if (child is View view)
			{
				// we can ignore the request if we are either fully constrained or when the size request changes and we were already fully constrained
				if ((trigger == InvalidationTrigger.MeasureChanged && view.Constraint == LayoutConstraint.Fixed) ||
					(trigger == InvalidationTrigger.SizeRequestChanged && view.ComputedConstraint == LayoutConstraint.Fixed))
				{
					return;
				}
				if (trigger == InvalidationTrigger.HorizontalOptionsChanged || trigger == InvalidationTrigger.VerticalOptionsChanged)
				{
					ComputeConstraintForView(view);
				}
			}

			InvalidateMeasureLegacy(trigger, depth, int.MaxValue);
		}

		// This lets us override the rules for invalidation on MAUI controls that unfortunately still inheirt from the legacy layout
		private protected virtual void InvalidateMeasureLegacy(InvalidationTrigger trigger, int depth, int depthLeveltoInvalidate)
		{
			if (depth <= depthLeveltoInvalidate)
			{
				if (trigger == InvalidationTrigger.RendererReady)
				{
					InvalidateMeasureInternal(new InvalidationEventArgs(InvalidationTrigger.RendererReady, depth));
				}
				else
				{
					InvalidateMeasureInternal(new InvalidationEventArgs(InvalidationTrigger.MeasureChanged, depth));
				}
			}
			else
			{
				FireMeasureChanged(trigger, depth);
			}
		}

		internal override void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			base.OnIsVisibleChanged(oldValue, newValue);
			if (newValue)
			{
				if (_lastLayoutSize != new Size(Width, Height))
				{
					UpdateChildrenLayout();
				}
			}
		}

		static int GetElementDepth(Element view)
		{
			var result = 0;
			while (view.Parent != null)
			{
				result++;
				view = view.Parent;
			}
			return result;
		}

		void InternalChildrenOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Move)
			{
				return;
			}

			if (e.OldItems != null)
			{
				for (int i = 0; i < e.OldItems.Count; i++)
				{
					object item = e.OldItems[i];
					var v = item as View;
					if (v == null)
					{
						continue;
					}

					OnInternalRemoved(v, e.OldStartingIndex + i);
				}
			}

			if (e.NewItems != null)
			{
				for (int i = 0; i < e.NewItems.Count; i++)
				{
					object item = e.NewItems[i];
					var v = item as View;
					if (v == null)
					{
						continue;
					}

					if (item == this)
					{
						throw new InvalidOperationException("Cannot add self to own child collection.");
					}

					OnInternalAdded(v);
				}
			}
		}

		void OnInternalAdded(View view)
		{
			var parent = view.Parent as Layout;
			parent?.InternalChildren.Remove(view);

			OnChildAdded(view);
			if (ShouldInvalidateOnChildAdded(view))
			{
				InvalidateLayout();
			}
		}

		void OnInternalRemoved(View view, int oldIndex)
		{
			OnChildRemoved(view, oldIndex);
			if (ShouldInvalidateOnChildRemoved(view))
			{
				InvalidateLayout();
			}
		}

		bool ShouldLayoutChildren()
		{
			if (Width <= 0 || Height <= 0 || !LogicalChildrenInternal.Any() || !IsVisible || !IsPlatformStateConsistent || DisableLayout)
			{
				return false;
			}

			foreach (Element element in VisibleDescendants())
			{
				var visual = element as VisualElement;
				if (visual == null || !visual.IsVisible)
				{
					continue;
				}

				if (!visual.IsPlatformEnabled || !visual.IsPlatformStateConsistent)
				{
					return false;
				}
			}
			return true;
		}

		protected override void InvalidateMeasureOverride()
		{
			base.InvalidateMeasureOverride();

			foreach (var child in ((IElementController)this).LogicalChildren)
			{
				if (child is IView fe)
				{
					fe.InvalidateMeasure();
				}
			}
		}

		protected override Size ArrangeOverride(Rect bounds)
		{
			base.ArrangeOverride(bounds);

			// The SholdLayoutChildren check will catch impossible sizes (negative widths/heights), not-yet-loaded controls,
			// and other weirdness that comes from the legacy layouts trying to run layout before the native side is ready. 
			if (!ShouldLayoutChildren())
			{
				return bounds.Size;
			}

			UpdateChildrenLayout();

			return Frame.Size;
		}

		/// <inheritdoc cref="ICrossPlatformLayout.CrossPlatformMeasure(double, double)" />
		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			return OnMeasure(widthConstraint, heightConstraint).Request;
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <inheritdoc cref="ICrossPlatformLayout.CrossPlatformArrange(Rect)" />
		public Size CrossPlatformArrange(Rect bounds)
		{
			UpdateChildrenLayout();

			return Frame.Size;
		}

		static void OnCascadeInputTransparentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			// We only need to update if the cascade changes anything, namely when InputTransparent=true.
			// When InputTransparent=false, then the cascade property has no effect.
			if (bindable is Layout layout && layout.InputTransparent)
			{
				layout.RefreshInputTransparentProperty();
			}
		}
	}
}
