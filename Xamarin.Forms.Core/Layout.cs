using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[ContentProperty("Children")]
	public abstract class Layout<T> : Layout, IViewContainer<T> where T : View
	{
		readonly ElementCollection<T> _children;

		protected Layout()
		{
			_children = new ElementCollection<T>(InternalChildren);
		}

		public new IList<T> Children
		{
			get { return _children; }
		}

		protected virtual void OnAdded(T view)
		{
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);

			var typedChild = child as T;
			if (typedChild != null)
				OnAdded(typedChild);
		}

		protected override void OnChildRemoved(Element child)
		{
			base.OnChildRemoved(child);

			var typedChild = child as T;
			if (typedChild != null)
				OnRemoved(typedChild);
		}

		protected virtual void OnRemoved(T view)
		{
		}
	}

	public abstract class Layout : View, ILayout, ILayoutController, IPaddingElement
	{
		public static readonly BindableProperty IsClippedToBoundsProperty = BindableProperty.Create("IsClippedToBounds", typeof(bool), typeof(Layout), false);

		public static readonly BindableProperty CascadeInputTransparentProperty = BindableProperty.Create(
			nameof(CascadeInputTransparent), typeof(bool), typeof(Layout), true);

		public static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		static IList<KeyValuePair<Layout, int>> s_resolutionList = new List<KeyValuePair<Layout, int>>();
		static bool s_relayoutInProgress;
		bool _allocatedFlag;

		bool _hasDoneLayout;
		Size _lastLayoutSize = new Size(-1, -1);

		ReadOnlyCollection<Element> _logicalChildren;

		protected Layout()
		{
			//if things were added in base ctor (through implicit styles), the items added aren't properly parented
			if (InternalChildren.Count > 0)
				InternalChildrenOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, InternalChildren));

			InternalChildren.CollectionChanged += InternalChildrenOnCollectionChanged;
		}

		public bool IsClippedToBounds
		{
			get { return (bool)GetValue(IsClippedToBoundsProperty); }
			set { SetValue(IsClippedToBoundsProperty, value); }
		}

		public Thickness Padding
		{
			get { return (Thickness)GetValue(PaddingElement.PaddingProperty); }
			set { SetValue(PaddingElement.PaddingProperty, value); }
		}

		public bool CascadeInputTransparent
		{
			get { return (bool)GetValue(CascadeInputTransparentProperty); }
			set { SetValue(CascadeInputTransparentProperty, value); }
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator()
		{
			return default(Thickness);
		}

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue)
		{
			UpdateChildrenLayout();
		}

		internal ObservableCollection<Element> InternalChildren { get; } = new ObservableCollection<Element>();

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal
		{
			get { return _logicalChildren ?? (_logicalChildren = new ReadOnlyCollection<Element>(InternalChildren)); }
		}

		public event EventHandler LayoutChanged;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public IReadOnlyList<Element> Children
		{
			get { return InternalChildren; }
		}

		public void ForceLayout()
		{
			SizeAllocated(Width, Height);
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		public sealed override SizeRequest GetSizeRequest(double widthConstraint, double heightConstraint)
		{
			SizeRequest size = base.GetSizeRequest(widthConstraint - Padding.HorizontalThickness, heightConstraint - Padding.VerticalThickness);
			return new SizeRequest(new Size(size.Request.Width + Padding.HorizontalThickness, size.Request.Height + Padding.VerticalThickness),
				new Size(size.Minimum.Width + Padding.HorizontalThickness, size.Minimum.Height + Padding.VerticalThickness));
		}

		public static void LayoutChildIntoBoundingRegion(VisualElement child, Rectangle region)
		{
			var parent = child.Parent as IFlowDirectionController;
			bool isRightToLeft = false;
			if (parent != null && (isRightToLeft = parent.EffectiveFlowDirection.IsRightToLeft()))
				region = new Rectangle(parent.Width - region.Right, region.Y, region.Width, region.Height);

			var view = child as View;
			if (view == null)
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

		protected override void OnSizeAllocated(double width, double height)
		{
			_allocatedFlag = true;
			base.OnSizeAllocated(width, height);
			UpdateChildrenLayout();
		}

		protected virtual bool ShouldInvalidateOnChildAdded(View child)
		{
			return true;
		}

		protected virtual bool ShouldInvalidateOnChildRemoved(View child)
		{
			return true;
		}

		protected void UpdateChildrenLayout()
		{
			_hasDoneLayout = true;

			if (!ShouldLayoutChildren())
				return;

			var oldBounds = new Rectangle[LogicalChildrenInternal.Count];
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

			LayoutChildren(x, y, w, h);

			for (var i = 0; i < oldBounds.Length; i++)
			{
				Rectangle oldBound = oldBounds[i];
				Rectangle newBound = ((VisualElement)LogicalChildrenInternal[i]).Bounds;
				if (oldBound != newBound)
				{
					LayoutChanged?.Invoke(this, EventArgs.Empty);
					return;
				}
			}

			_lastLayoutSize = new Size(width, height);
		}

		internal static void LayoutChildIntoBoundingRegion(View child, Rectangle region, SizeRequest childSizeRequest)
		{
			var parent = child.Parent as IFlowDirectionController;
			bool isRightToLeft = false;
			if (parent != null && (isRightToLeft = parent.EffectiveFlowDirection.IsRightToLeft()))
				region = new Rectangle(parent.Width - region.Right, region.Y, region.Width, region.Height);

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
			ReadOnlyCollection<Element> children = LogicalChildrenInternal;
			int count = children.Count;
			for (var index = 0; index < count; index++)
			{
				var v = LogicalChildrenInternal[index] as VisualElement;
				if (v != null && v.IsVisible && (!v.IsPlatformEnabled || !v.IsNativeStateConsistent))
					return;
			}

			var view = child as View;
			if (view != null)
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

			_allocatedFlag = false;
			if (trigger == InvalidationTrigger.RendererReady)
			{
				InvalidateMeasureInternal(InvalidationTrigger.RendererReady);
			}
			else
			{
				InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
			}

			s_resolutionList.Add(new KeyValuePair<Layout, int>(this, GetElementDepth(this)));
			if (!s_relayoutInProgress)
			{
				s_relayoutInProgress = true;
				Device.BeginInvokeOnMainThread(() =>
				{
					// if thread safety mattered we would need to lock this and compareexchange above
					IList<KeyValuePair<Layout, int>> copy = s_resolutionList;
					s_resolutionList = new List<KeyValuePair<Layout, int>>();
					s_relayoutInProgress = false;

					foreach (KeyValuePair<Layout, int> kvp in copy.OrderBy(kvp => kvp.Value))
					{
						Layout layout = kvp.Key;
						double width = layout.Width, height = layout.Height;
						if (!layout._allocatedFlag && width >= 0 && height >= 0)
						{
							layout.SizeAllocated(width, height);
						}
					}
				});
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
				foreach (object item in e.OldItems)
				{
					var v = item as View;
					if (v == null)
						continue;

					OnInternalRemoved(v);
				}
			}

			if (e.NewItems != null)
			{
				foreach (object item in e.NewItems)
				{
					var v = item as View;
					if (v == null)
						continue;

					if (item == this)
						throw new InvalidOperationException("Can not add self to own child collection.");

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

		void OnInternalRemoved(View view)
		{
			view.MeasureInvalidated -= OnChildMeasureInvalidated;

			OnChildRemoved(view);
			if (ShouldInvalidateOnChildRemoved(view))
				InvalidateLayout();
		}

		bool ShouldLayoutChildren()
		{
			if (Width <= 0 || Height <= 0 || !LogicalChildrenInternal.Any() || !IsVisible || !IsNativeStateConsistent || DisableLayout)
				return false;

			foreach (Element element in VisibleDescendants())
			{
				var visual = element as VisualElement;
				if (visual == null || !visual.IsVisible)
					continue;

				if (!visual.IsPlatformEnabled || !visual.IsNativeStateConsistent)
				{
					return false;
				}
			}
			return true;
		}
	}
}
