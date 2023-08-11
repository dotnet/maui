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
	[ContentProperty(nameof(Children))]
	public abstract partial class Layout<T> : Layout, Microsoft.Maui.ILayout, ILayoutManager, IBindableLayout, IViewContainer<T> where T : View
	{
		readonly ElementCollection<T> _children;

		protected Layout() => _children = new ElementCollection<T>(InternalChildren);

		public new IList<T> Children => _children;

		public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;

		IList IBindableLayout.Children => _children;

		bool ISafeAreaView.IgnoreSafeArea => false;

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			if (child is T typedChild)
				OnAdded(typedChild);
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);

			if (child is T typedChild)
				OnRemoved(typedChild);
		}

		protected virtual void OnAdded(T view)
		{
		}

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
				InternalChildrenOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, InternalChildren));

			InternalChildren.CollectionChanged += InternalChildrenOnCollectionChanged;
		}

		public bool IsClippedToBounds
		{
			get => (bool)GetValue(IsClippedToBoundsProperty);
			set => SetValue(IsClippedToBoundsProperty, value);
		}

		public Thickness Padding
		{
			get => (Thickness)GetValue(PaddingElement.PaddingProperty);
			set => SetValue(PaddingElement.PaddingProperty, value);
		}

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

		public event EventHandler LayoutChanged;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public IReadOnlyList<Element> Children => InternalChildren;

		public void ForceLayout() => SizeAllocated(Width, Height);

		IReadOnlyList<Maui.IVisualTreeElement> IVisualTreeElement.GetVisualChildren() => Children.ToList().AsReadOnly();

		public override SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
		{
			SizeRequest size = base.Measure(widthConstraint - Padding.HorizontalThickness, heightConstraint - Padding.VerticalThickness, flags);
			return new SizeRequest(new Size(size.Request.Width + Padding.HorizontalThickness, size.Request.Height + Padding.VerticalThickness),
				new Size(size.Minimum.Width + Padding.HorizontalThickness, size.Minimum.Height + Padding.VerticalThickness));
		}

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
				SizeRequest request = child.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
				double diff = Math.Max(0, region.Width - request.Request.Width);
				double horizontalAlign = horizontalOptions.Alignment.ToDouble();
				if (isRightToLeft)
					horizontalAlign = 1 - horizontalAlign;
				region.X += (int)(diff * horizontalAlign);
				region.Width -= diff;
			}

			LayoutOptions verticalOptions = view.VerticalOptions;
			if (verticalOptions.Alignment != LayoutAlignment.Fill)
			{
				SizeRequest request = child.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
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

		public void LowerChild(View view)
		{
			if (!InternalChildren.Contains(view) || InternalChildren.First() == view)
				return;

			InternalChildren.Move(InternalChildren.IndexOf(view), 0);
			OnChildrenReordered();
		}

		public void RaiseChild(View view)
		{
			if (!InternalChildren.Contains(view) || InternalChildren.Last() == view)
				return;

			InternalChildren.Move(InternalChildren.IndexOf(view), InternalChildren.Count - 1);
			OnChildrenReordered();
		}

		protected virtual void InvalidateLayout()
		{
			_hasDoneLayout = false;
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			if (!_hasDoneLayout)
				ForceLayout();
		}

		protected abstract void LayoutChildren(double x, double y, double width, double height);

		protected void OnChildMeasureInvalidated(object sender, EventArgs e)
		{
			InvalidationTrigger trigger = (e as InvalidationEventArgs)?.Trigger ?? InvalidationTrigger.Undefined;
			OnChildMeasureInvalidated((VisualElement)sender, trigger);
			OnChildMeasureInvalidated();
		}

		protected virtual void OnChildMeasureInvalidated()
		{
		}

		Size IView.Measure(double widthConstraint, double heightConstraint)
		{
			return MeasureOverride(widthConstraint, heightConstraint);
		}

		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			var sansMargins = OnMeasure(widthConstraint, heightConstraint).Request;
			DesiredSize = new Size(sansMargins.Width + Margin.HorizontalThickness, sansMargins.Height + Margin.VerticalThickness);
			return DesiredSize;
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			UpdateChildrenLayout();
		}

		protected virtual bool ShouldInvalidateOnChildAdded(View child) => true;

		protected virtual bool ShouldInvalidateOnChildRemoved(View child) => true;

		protected void UpdateChildrenLayout()
		{
			_hasDoneLayout = true;

			if (!ShouldLayoutChildren())
				return;

			var oldBounds = new Rect[LogicalChildrenInternal.Count];
			for (var index = 0; index < oldBounds.Length; index++)
			{
				var c = (VisualElement)LogicalChildrenInternal[index];
				oldBounds[index] = c.Bounds;
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
				CompressedLayout.SetHeadlessOffset((VisualElement)LogicalChildrenInternal[i], isHeadless ? new Point(headlessOffset.X + Bounds.X, headlessOffset.Y + Bounds.Y) : new Point());

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

		internal static void LayoutChildIntoBoundingRegion(View child, Rect region, SizeRequest childSizeRequest)
		{
			bool isRightToLeft = false;
			if (child.Parent is IFlowDirectionController parent && (isRightToLeft = parent.ApplyEffectiveFlowDirectionToChildContainer && parent.EffectiveFlowDirection.IsRightToLeft()))
				region = new Rect(parent.Width - region.Right, region.Y, region.Width, region.Height);

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
					SizeRequest request = canUseAlreadyDoneRequest ? childSizeRequest : child.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
					double diff = Math.Max(0, region.Width - request.Request.Width);
					double horizontalAlign = horizontalOptions.Alignment.ToDouble();
					if (isRightToLeft)
						horizontalAlign = 1 - horizontalAlign;
					region.X += (int)(diff * horizontalAlign);
					region.Width -= diff;
				}

				LayoutOptions verticalOptions = child.VerticalOptions;
				if (verticalOptions.Alignment != LayoutAlignment.Fill)
				{
					SizeRequest request = canUseAlreadyDoneRequest ? childSizeRequest : child.Measure(region.Width, region.Height, MeasureFlags.IncludeMargins);
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

		internal virtual void OnChildMeasureInvalidated(VisualElement child, InvalidationTrigger trigger)
		{
			IReadOnlyList<Element> children = LogicalChildrenInternal;
			int count = children.Count;
			for (var index = 0; index < count; index++)
			{
				if (LogicalChildrenInternal[index] is VisualElement v && v.IsVisible && (!v.IsPlatformEnabled || !v.IsPlatformStateConsistent))
					return;
			}

			if (child is View view)
			{
				// we can ignore the request if we are either fully constrained or when the size request changes and we were already fully constrainted
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

			if (trigger == InvalidationTrigger.RendererReady)
			{
				InvalidateMeasureInternal(InvalidationTrigger.RendererReady);
			}
			else
			{
				InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
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
						continue;

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
						continue;

					if (item == this)
						throw new InvalidOperationException("Cannot add self to own child collection.");

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
				InvalidateLayout();

			view.MeasureInvalidated += OnChildMeasureInvalidated;
		}

		void OnInternalRemoved(View view, int oldIndex)
		{
			view.MeasureInvalidated -= OnChildMeasureInvalidated;

			OnChildRemoved(view, oldIndex);
			if (ShouldInvalidateOnChildRemoved(view))
				InvalidateLayout();
		}

		bool ShouldLayoutChildren()
		{
			if (Width <= 0 || Height <= 0 || !LogicalChildrenInternal.Any() || !IsVisible || !IsPlatformStateConsistent || DisableLayout)
				return false;

			foreach (Element element in VisibleDescendants())
			{
				var visual = element as VisualElement;
				if (visual == null || !visual.IsVisible)
					continue;

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
				return bounds.Size;

			UpdateChildrenLayout();

			return Frame.Size;
		}

		public Size CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			return OnMeasure(widthConstraint, heightConstraint).Request;
		}

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
