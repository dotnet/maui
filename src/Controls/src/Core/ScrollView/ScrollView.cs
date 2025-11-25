#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="Type[@FullName='Microsoft.Maui.Controls.ScrollView']/Docs/*" />
	[ContentProperty(nameof(Content))]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	[ElementHandler<ScrollViewHandler>]
#pragma warning disable CS0618 // Type or member is obsolete
	public partial class ScrollView : Compatibility.Layout, ILayout, ILayoutController, IPaddingElement, IView, IVisualTreeElement, IInputTransparentContainerElement, IScrollViewController, IElementConfiguration<ScrollView>, IFlowDirectionController, IScrollView, IContentView, ISafeAreaElement, ISafeAreaView2
#pragma warning restore CS0618 // Type or member is obsolete
	{
		#region IScrollViewController

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='LayoutAreaOverride']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This API doesn't do anything", true)]
		public Rect LayoutAreaOverride
		{
			get => _layoutAreaOverride;
			set
			{
				if (_layoutAreaOverride == value)
					return;
				_layoutAreaOverride = value;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

		ScrollToRequestedEventArgs _pendingScrollToRequested;

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler is not null && _pendingScrollToRequested is not null)
			{
				OnScrollToRequested(_pendingScrollToRequested);
				_pendingScrollToRequested = null;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='GetScrollPositionForElement']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Point GetScrollPositionForElement(VisualElement item, ScrollToPosition pos)
		{
			ScrollToPosition position = pos;
			double y = GetCoordinate(item, "Y", 0);
			double x = GetCoordinate(item, "X", 0);

			if (position == ScrollToPosition.MakeVisible)
			{
				var scrollBounds = new Rect(ScrollX, ScrollY, Width, Height);
				var itemBounds = new Rect(x, y, item.Width, item.Height);
				if (scrollBounds.Contains(itemBounds))
					return new Point(ScrollX, ScrollY);
				switch (Orientation)
				{
					case ScrollOrientation.Vertical:
						position = y > ScrollY ? ScrollToPosition.End : ScrollToPosition.Start;
						break;
					case ScrollOrientation.Horizontal:
						position = x > ScrollX ? ScrollToPosition.End : ScrollToPosition.Start;
						break;
					case ScrollOrientation.Both:
						position = x > ScrollX || y > ScrollY ? ScrollToPosition.End : ScrollToPosition.Start;
						break;
				}
			}
			switch (position)
			{
				case ScrollToPosition.Center:
					y = y - Height / 2 + item.Height / 2;
					x = x - Width / 2 + item.Width / 2;
					break;
				case ScrollToPosition.End:
					y = y - Height + item.Height;
					x = x - Width + item.Width;
					break;
			}
			return new Point(x, y);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='SendScrollFinished']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendScrollFinished()
		{
			_scrollCompletionSource?.TrySetResult(true);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='SetScrolledPosition']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SetScrolledPosition(double x, double y)
		{
			if (ScrollX == x && ScrollY == y)
				return;

			ScrollX = x;
			ScrollY = y;

			Scrolled?.Invoke(this, new ScrolledEventArgs(x, y));
		}

		#endregion IScrollViewController

		/// <summary>Bindable property for <see cref="Orientation"/>.</summary>
		public static readonly BindableProperty OrientationProperty = BindableProperty.Create(nameof(Orientation), typeof(ScrollOrientation), typeof(ScrollView), ScrollOrientation.Vertical);

		static readonly BindablePropertyKey ScrollXPropertyKey = BindableProperty.CreateReadOnly(nameof(ScrollX), typeof(double), typeof(ScrollView), 0d);

		/// <summary>Bindable property for <see cref="ScrollX"/>.</summary>
		public static readonly BindableProperty ScrollXProperty = ScrollXPropertyKey.BindableProperty;

		static readonly BindablePropertyKey ScrollYPropertyKey = BindableProperty.CreateReadOnly(nameof(ScrollY), typeof(double), typeof(ScrollView), 0d);

		/// <summary>Bindable property for <see cref="ScrollY"/>.</summary>
		public static readonly BindableProperty ScrollYProperty = ScrollYPropertyKey.BindableProperty;

		static readonly BindablePropertyKey ContentSizePropertyKey = BindableProperty.CreateReadOnly(nameof(ContentSize), typeof(Size), typeof(ScrollView), default(Size));

		/// <summary>Bindable property for <see cref="ContentSize"/>.</summary>
		public static readonly BindableProperty ContentSizeProperty = ContentSizePropertyKey.BindableProperty;

		readonly Lazy<PlatformConfigurationRegistry<ScrollView>> _platformConfigurationRegistry;

		/// <summary>Bindable property for <see cref="HorizontalScrollBarVisibility"/>.</summary>
		public static readonly BindableProperty HorizontalScrollBarVisibilityProperty = BindableProperty.Create(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ScrollView), ScrollBarVisibility.Default);

		/// <summary>Bindable property for <see cref="VerticalScrollBarVisibility"/>.</summary>
		public static readonly BindableProperty VerticalScrollBarVisibilityProperty = BindableProperty.Create(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ScrollView), ScrollBarVisibility.Default);

		/// <summary>Bindable property for <see cref="SafeAreaEdges"/>.</summary>
		public static readonly BindableProperty SafeAreaEdgesProperty = SafeAreaElement.SafeAreaEdgesProperty;

		View _content;
		TaskCompletionSource<bool> _scrollCompletionSource;
		Rect _layoutAreaOverride;
		IReadOnlyList<Element> ILayoutController.Children => LogicalChildrenInternal;

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public View Content
		{
			get { return _content; }
			set
			{
				if (_content == value)
					return;

				OnPropertyChanging();
				if (_content is not null)
				{
					_content.SizeChanged -= ContentSizeChanged;
					RemoveLogicalChild(_content);
				}
				_content = value;
				if (_content is not null)
				{
					AddLogicalChild(_content);
					_content.SizeChanged += ContentSizeChanged;
				}

				OnPropertyChanged();
				Handler?.UpdateValue(nameof(Content));
			}
		}

		/// <summary>Bindable property for <see cref="CascadeInputTransparent"/>.</summary>
		public new static readonly BindableProperty CascadeInputTransparentProperty = InputTransparentContainerElement.CascadeInputTransparentProperty;

		/// <summary>
		/// Gets or sets a value that controls whether child elements
		/// inherit the input transparency of this layout when the transparency is <see langword="true"/>.
		/// </summary>
		/// <value>
		/// <see langword="true" /> to cause child elements to inherit the input transparency of this layout,
		/// when this layout's <see cref="VisualElement.InputTransparent" /> property is <see langword="true" />.
		/// <see langword="false" /> to cause child elements to ignore the input transparency of this layout.
		/// </value>
		public new bool CascadeInputTransparent
		{
			get => (bool)GetValue(CascadeInputTransparentProperty);
			set => SetValue(CascadeInputTransparentProperty, value);
		}

		/// <summary>Bindable property for <see cref="Padding"/>.</summary>
		public new static readonly BindableProperty PaddingProperty = PaddingElement.PaddingProperty;

		/// <inheritdoc cref="IPaddingElement.Padding"/>
		public new Thickness Padding
		{
			get => (Thickness)GetValue(PaddingProperty);
			set => SetValue(PaddingProperty, value);
		}

		Thickness IPaddingElement.PaddingDefaultValueCreator() => default(Thickness);

		void IPaddingElement.OnPaddingPropertyChanged(Thickness oldValue, Thickness newValue) => InvalidateMeasure();

		void ContentSizeChanged(object sender, EventArgs e)
		{
			var view = (sender as IView);
			if (view is null)
			{
				ContentSize = Size.Zero;
				return;
			}

			var margin = view.Margin;
			var frameSize = view.Frame.Size;

			// The ContentSize includes the margins for the content
			ContentSize = new Size(frameSize.Width + margin.HorizontalThickness,
				frameSize.Height + margin.VerticalThickness);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ContentSize']/Docs/*" />
		public Size ContentSize
		{
			get { return (Size)GetValue(ContentSizeProperty); }
			private set { SetValue(ContentSizePropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='Orientation']/Docs/*" />
		public ScrollOrientation Orientation
		{
			get { return (ScrollOrientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ScrollX']/Docs/*" />
		public double ScrollX
		{
			get { return (double)GetValue(ScrollXProperty); }
			private set { SetValue(ScrollXPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ScrollY']/Docs/*" />
		public double ScrollY
		{
			get { return (double)GetValue(ScrollYProperty); }
			private set { SetValue(ScrollYPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='HorizontalScrollBarVisibility']/Docs/*" />
		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
			set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='VerticalScrollBarVisibility']/Docs/*" />
		public ScrollBarVisibility VerticalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
			set { SetValue(VerticalScrollBarVisibilityProperty, value); }
		}

		/// <summary>
		/// Gets or sets the safe area edges to obey for this scroll view.
		/// The default value is SafeAreaEdges.Default (None - edge to edge).
		/// </summary>
		/// <remarks>
		/// This property controls which edges of the scroll view should obey safe area insets.
		/// Use SafeAreaRegions.None for edge-to-edge content, SafeAreaRegions.All to obey all safe area insets, 
		/// SafeAreaRegions.Container for content that flows under keyboard but stays out of bars/notch, or SafeAreaRegions.SoftInput for keyboard-aware behavior.
		/// </remarks>
		public SafeAreaEdges SafeAreaEdges
		{
			get => (SafeAreaEdges)GetValue(SafeAreaElement.SafeAreaEdgesProperty);
			set => SetValue(SafeAreaElement.SafeAreaEdgesProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ScrollView()
		{
			_platformConfigurationRegistry = new Lazy<PlatformConfigurationRegistry<ScrollView>>(() => new PlatformConfigurationRegistry<ScrollView>(this));
		}

		public event EventHandler<ScrolledEventArgs> Scrolled;

		/// <inheritdoc/>
		public IPlatformElementConfiguration<T, ScrollView> On<T>() where T : IConfigPlatform
		{
			return _platformConfigurationRegistry.Value.On<T>();
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ScrollToAsync'][1]/Docs/*" />
		public Task ScrollToAsync(double x, double y, bool animated)
		{
			if (Orientation == ScrollOrientation.Neither)
			{
				return Task.FromResult(false);
			}

			var args = new ScrollToRequestedEventArgs(x, y, animated);
			OnScrollToRequested(args);
			return _scrollCompletionSource.Task;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ScrollToAsync'][2]/Docs/*" />
		public Task ScrollToAsync(Element element, ScrollToPosition position, bool animated)
		{
			if (Orientation == ScrollOrientation.Neither)
			{
				return Task.FromResult(false);
			}

			if (!Enum.IsDefined(typeof(ScrollToPosition), position))
			{
				throw new ArgumentException("position is not a valid ScrollToPosition", nameof(position));
			}

			if (element is null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			if (!CheckElementBelongsToScrollViewer(element))
			{
				throw new ArgumentException("element does not belong to this ScrollView", nameof(element));
			}

			var args = new ScrollToRequestedEventArgs(element, position, animated);
			OnScrollToRequested(args);
			return _scrollCompletionSource.Task;
		}

		bool IFlowDirectionController.ApplyEffectiveFlowDirectionToChildContainer => false;

		protected override LayoutConstraint ComputeConstraintForView(View view)
		{
			switch (Orientation)
			{
				case ScrollOrientation.Horizontal:
					LayoutOptions vOptions = view.VerticalOptions;
					if (vOptions.Alignment == LayoutAlignment.Fill && (Constraint & LayoutConstraint.VerticallyFixed) != 0)
					{
						return LayoutConstraint.VerticallyFixed;
					}
					break;
				case ScrollOrientation.Vertical:
					LayoutOptions hOptions = view.HorizontalOptions;
					if (hOptions.Alignment == LayoutAlignment.Fill && (Constraint & LayoutConstraint.HorizontallyFixed) != 0)
					{
						return LayoutConstraint.HorizontallyFixed;
					}
					break;
				case ScrollOrientation.Both:
					return LayoutConstraint.None;
			}
			return LayoutConstraint.None;
		}

		bool CheckElementBelongsToScrollViewer(Element element)
		{
			return Equals(element, this) || element.RealParent is not null && CheckElementBelongsToScrollViewer(element.RealParent);
		}

		void CheckTaskCompletionSource()
		{
			if (_scrollCompletionSource is not null && _scrollCompletionSource.Task.Status == TaskStatus.Running)
			{
				_scrollCompletionSource.TrySetCanceled();
			}
			_scrollCompletionSource = new TaskCompletionSource<bool>();
		}

		double GetCoordinate(Element item, string coordinateName, double coordinate)
		{
			if (item == this)
			{
				return coordinate;
			}

			coordinate += (double)typeof(VisualElement).GetProperty(coordinateName).GetValue(item, null);
			var visualParentElement = item.RealParent as VisualElement;
			return visualParentElement is not null ? GetCoordinate(visualParentElement, coordinateName, coordinate) : coordinate;
		}

		void OnScrollToRequested(ScrollToRequestedEventArgs e)
		{
			CheckTaskCompletionSource();
			ScrollToRequested?.Invoke(this, e);

			if (Handler is null)
			{
				_pendingScrollToRequested = e;
			}
			else
			{
				Handler.Invoke(nameof(IScrollView.RequestScrollTo), ConvertRequestMode(e).ToRequest());
			}
		}

		ScrollToRequestedEventArgs ConvertRequestMode(ScrollToRequestedEventArgs args)
		{
			if (args.Mode == ScrollToMode.Element && args.Element is VisualElement visualElement)
			{
				var point = GetScrollPositionForElement(visualElement, args.Position);
				var result = new ScrollToRequestedEventArgs(point.X, point.Y, args.ShouldAnimate);
				return result;
			}

			return args;
		}

		object IContentView.Content => Content;

		IView IContentView.PresentedContent => Content;

		double IScrollView.HorizontalOffset
		{
			get => ScrollX;
			set
			{
				if (ScrollX != value)
				{
					SetScrolledPosition(value, ScrollY);
				}
			}
		}

		double IScrollView.VerticalOffset
		{
			get => ScrollY;
			set
			{
				if (ScrollY != value)
				{
					SetScrolledPosition(ScrollX, value);
				}
			}
		}

		void IScrollView.RequestScrollTo(double horizontalOffset, double verticalOffset, bool instant)
		{
			var request = new ScrollToRequest(horizontalOffset, verticalOffset, instant);
			Handler?.Invoke(nameof(IScrollView.RequestScrollTo), request);
		}

		void IScrollView.ScrollFinished() => SendScrollFinished();


		// Don't delete this override. At some point in the future we'd like to delete Compatibility.Layout
		// and this is the only way to ensure binary compatibility with code that's already compiled against MAUI
		// and is overriding MeasureOverride.
		protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			return this.ComputeDesiredSize(widthConstraint, heightConstraint);
		}

		Size ICrossPlatformLayout.CrossPlatformMeasure(double widthConstraint, double heightConstraint)
		{
			if ((this as IContentView)?.PresentedContent is not IView content)
			{
				ContentSize = Size.Zero;
				return ContentSize;
			}

			switch (Orientation)
			{
				case ScrollOrientation.Horizontal:
					widthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Neither:
				case ScrollOrientation.Both:
					heightConstraint = double.PositiveInfinity;
					widthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Vertical:
				default:
					heightConstraint = double.PositiveInfinity;
					break;
			}

			content.Measure(widthConstraint, heightConstraint);
			return content.DesiredSize;
		}


		// Don't delete this override. At some point in the future we'd like to delete Compatibility.Layout
		// and this is the only way to ensure binary compatibility with code that's already compiled against MAUI
		// and is overriding ArrangeOverride.
		protected override Size ArrangeOverride(Rect bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.PlatformArrange(Frame);

			return Frame.Size;
		}

		// Don't delete this override. At some point in the future we'd like to delete Compatibility.Layout
		// and this is the only way to ensure binary compatibility with code that's already compiled against MAUI
		// and is overriding OnSizeAllocated.
		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
		}

		Size ICrossPlatformLayout.CrossPlatformArrange(Rect bounds)
		{
			if (this is IScrollView scrollView)
			{
				return scrollView.ArrangeContentUnbounded(bounds);
			}

			return bounds.Size;
		}

		Size IContentView.CrossPlatformMeasure(double widthConstraint, double heightConstraint) =>
			((ICrossPlatformLayout)this).CrossPlatformMeasure(widthConstraint, heightConstraint);

		Size IContentView.CrossPlatformArrange(Rect bounds) =>
			((ICrossPlatformLayout)this).CrossPlatformArrange(bounds);

		SafeAreaEdges ISafeAreaElement.SafeAreaEdgesDefaultValueCreator()
		{
			return SafeAreaEdges.Default;
		}

		/// <inheritdoc cref="ISafeAreaView2.SafeAreaInsets"/>
		Thickness ISafeAreaView2.SafeAreaInsets
		{
			set
			{
				// For ScrollView, we don't need to store the SafeAreaInsets
				// The platform-specific MauiScrollView handles this
			}
		}

		/// <inheritdoc cref="ISafeAreaView2.GetSafeAreaRegionsForEdge"/>
		SafeAreaRegions ISafeAreaView2.GetSafeAreaRegionsForEdge(int edge)
		{
			// Use direct property 
			var regionForEdge = SafeAreaEdges.GetEdge(edge);

			// For ScrollView, return Default behavior as-is (it's special)
			return regionForEdge;
		}

		private protected override string GetDebuggerDisplay()
		{
			var debugText = DebuggerDisplayHelpers.GetDebugText(nameof(Content), Content);
			return $"{base.GetDebuggerDisplay()}, {debugText}";
		}

		[Obsolete("Use ArrangeOverride instead")]
		protected override void LayoutChildren(double x, double y, double width, double height)
		{
		}

		[Obsolete("Use Measure with no flags.")]
		public override SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
		{
			return base.Measure(widthConstraint, heightConstraint);
		}


		/// <summary>
		/// Sends a child to the back of the visual stack.
		/// </summary>
		/// <param name="view">The view to lower in the visual stack.</param>
		/// <remarks>Children are internally stored in visual stack order.
		/// This means that raising or lowering a child also changes the order in which the children are enumerated.</remarks>
		[Obsolete("Use the ZIndex Property instead")]
		public new void LowerChild(View view)
		{
			base.LowerChild(view);
		}

		/// <summary>
		/// Sends a child to the front of the visual stack.
		/// </summary>
		/// <param name="view">The view to raise in the visual stack.</param>
		/// <remarks>Children are internally stored in visual stack order.
		/// This means that raising or lowering a child also changes the order in which the children are enumerated.</remarks>
		[Obsolete("Use the ZIndex Property instead")]
		public new void RaiseChild(View view)
		{
			base.RaiseChild(view);
		}

		/// <summary>
		/// Invalidates the current layout.
		/// </summary>
		/// <remarks>Calling this method will invalidate the measure and triggers a new layout cycle.</remarks>
		[Obsolete("Use InvalidateMeasure depending on your scenario")]
		protected override void InvalidateLayout()
		{
			base.InvalidateLayout();
		}

		/// <summary>
		/// Invoked whenever a child of the layout has emitted <see cref="VisualElement.MeasureInvalidated" />.
		/// Implement this method to add class handling for this event.
		/// </summary>
		[Obsolete("Subscribe to the MeasureInvalidated Event on the Children.")]
		protected override void OnChildMeasureInvalidated()
		{
		}

		/// <summary>
		/// If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.
		/// </summary>
		/// <param name="child">The child for which to specify whether or not to track invalidation.</param>
		/// <returns><see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" />, otherwise <see langword="false"/>.</returns>
		[Obsolete("If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.")]
		protected override bool ShouldInvalidateOnChildAdded(View child) => true;

		/// <summary>
		/// If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.
		/// </summary>
		/// <param name="child">The child for which to specify whether or not to track invalidation.</param>
		/// <returns><see langword="true" /> if <paramref name="child" /> should call <see cref="VisualElement.InvalidateMeasure" />, otherwise <see langword="false"/>.</returns>
		[Obsolete("If you want to influence invalidation override InvalidateMeasureOverride. This method will no longer work on .NET 10 and later.")]
		protected override bool ShouldInvalidateOnChildRemoved(View child) => true;

		/// <summary>
		/// Use InvalidateMeasure depending on your scenario. This method will no longer work on .NET 10 and later.
		/// </summary>
		[Obsolete("Use InvalidateMeasure depending on your scenario. This method will no longer work on .NET 10 and later.")]
		protected new void UpdateChildrenLayout()
		{
		}

		[Obsolete("Use MeasureOverride instead")]
		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			return base.OnMeasure(widthConstraint, heightConstraint);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use IVisualTreeElement.GetVisualChildren() instead.", true)]
		public new IReadOnlyList<Element> Children => base.Children;
	}
}