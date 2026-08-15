#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Represents a view that is capable of scrolling if its content requires it.
	/// </summary>
	[ContentProperty(nameof(Content))]
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
#pragma warning disable CS0618 // Type or member is obsolete
	public partial class ScrollView : Compatibility.Layout, ILayout, ILayoutController, IPaddingElement, IView, IVisualTreeElement, IInputTransparentContainerElement, IScrollViewController, IElementConfiguration<ScrollView>, IFlowDirectionController, IScrollView, IScrollOffsetReceiver, IContentView, ISafeAreaElement, ISafeAreaView2
#pragma warning restore CS0618 // Type or member is obsolete
	{
		#region IScrollViewController

		/// <summary>
		/// Gets or sets the layout area override for the scroll view.
		/// </summary>
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
		bool _replayPendingScrollToRequestedEvent;

		// A parked element request lives exactly as long as the task the caller is awaiting.
		// It leaves the park in one of two ways, and no other: the arrange arrives and the
		// request is replayed against real geometry (the task completes with the scroll),
		// or the view's lifecycle ends — its handler goes away or it is removed from the
		// tree — and the request is dropped (the task completes without a scroll). Nothing
		// releases the task while the request is still parked, so a completed await never
		// scrolls later; and nothing but a lifecycle end drops the request, so a view that is
		// merely hidden (a collapsed branch, an unselected tab) still scrolls to the element
		// when it is eventually arranged. A view that stays attached and is never arranged
		// keeps the task pending — that is the contract, not a leak: the task completes when
		// the scroll happens or the view is torn down.
		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			if (Handler is null)
			{
				// The handler went away with a request still queued, so nothing will ever
				// dispatch it; Core does the same for its own pending request on disconnect
				DropPendingScrollToRequest();
				return;
			}

			DispatchPendingScrollToRequest();
		}

		private protected override void OnParentChangedCore()
		{
			base.OnParentChangedCore();

			// Removed from the tree: no arrange will come from a parent that no longer
			// exists, and the handler is not necessarily disconnected by the removal, so
			// this is the other lifecycle end that drops a parked request
			if (RealParent is null)
			{
				DropPendingScrollToRequest();
			}
		}

		void DropPendingScrollToRequest()
		{
			if (_pendingScrollToRequested is null)
			{
				return;
			}

			_pendingScrollToRequested = null;
			// A stale replay flag from a pre-handler park must not carry over to a later request
			_replayPendingScrollToRequestedEvent = false;

			// Safe to complete here even though this runs from a lifecycle mutation: the
			// completion source runs continuations asynchronously, so the caller never
			// resumes inline on this stack (see CheckTaskCompletionSource)
			SendScrollFinished();
		}

		void DispatchPendingScrollToRequest()
		{
			if (Handler is null || _pendingScrollToRequested is not { } pending)
			{
				return;
			}

			if (pending.Mode == ScrollToMode.Element)
			{
				if (!IsElementTargetGeometryReady())
				{
					// The request has to wait; OnSizeAllocated and ContentSizeChanged retry it.
					return;
				}

				// Those callbacks run while the pass that produced the sizes is still
				// arranging children, so resolve on the next tick, once positions are final.
				// Posting on every retry is deliberate: SendPendingScrollToRequest is a no-op
				// once the request has been sent or superseded, so a dropped callback cannot
				// wedge the request the way an "already queued" flag would.
				Dispatcher.Dispatch(SendPendingScrollToRequest);
				return;
			}

			SendPendingScrollToRequest();
		}

		// An element target is resolved against this ScrollView's geometry and the element's
		// position inside the arranged content. Before the first layout pass Width/Height are
		// still -1 (the never-arranged sentinel, for the content too), so a target computed
		// then is garbage. The content check must be "not yet arranged" rather than "arranged
		// to nothing": content can legitimately arrange to a zero size (a collapsed container),
		// and that raises no further callbacks — gating on the size would hang the caller's
		// task forever, while dispatching just clamps the target to the origin.
		bool IsElementTargetGeometryReady() =>
			Width >= 0 && Height >= 0 && Content is not ({ Width: < 0 } or { Height: < 0 });

		void SendPendingScrollToRequest()
		{
			if (Handler is null || _pendingScrollToRequested is not { } pending)
			{
				return;
			}

			_pendingScrollToRequested = null;

			// Replay without going through OnScrollToRequested: that would reset the
			// completion source and orphan the task the original caller is still awaiting.
			// The event is re-raised only for requests parked before the handler attached:
			// compatibility renderers subscribe to ScrollToRequested at attach and perform the
			// scroll from it, so they would otherwise never see the request. A request parked
			// with the handler present (waiting for element geometry) already notified its
			// subscribers at request time, and re-raising would double-notify them.
			if (_replayPendingScrollToRequestedEvent)
			{
				_replayPendingScrollToRequestedEvent = false;
				ScrollToRequested?.Invoke(this, pending);
			}

			Handler.Invoke(nameof(IScrollView.RequestScrollTo), ConvertRequestMode(pending).ToRequest());
		}


		/// <summary>
		/// Gets the scroll position for the specified element.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Point GetScrollPositionForElement(VisualElement item, ScrollToPosition pos)
		{
			ScrollToPosition position = pos;
			double y = GetCoordinate(item, "Y", 0);
			double x = GetCoordinate(item, "X", 0);

			// The scrollable viewport can be smaller than this ScrollView's frame: on iOS, content
			// insets (safe area, ContentInset) obscure part of the frame and (0,0) in scroll
			// coordinates is the inset rest position. Compute element targets against the effective
			// viewport so End/Center/MakeVisible land the element fully inside the visible region.
			// Part of those insets can be baked into the content itself (the platform arranged the
			// content inside safe-area-inset bounds): element coordinates already include that
			// padding, so targets shift back by it — the platform-inset part is instead
			// compensated when the request is translated to a native offset.
			var viewportInsets = GetVisibleViewportInsets();
			var contentInsets = GetContentCoordinateInsets();
			double viewportWidth = Math.Max(0, Width - viewportInsets.HorizontalThickness);
			double viewportHeight = Math.Max(0, Height - viewportInsets.VerticalThickness);

			if (position == ScrollToPosition.MakeVisible)
			{
				// In content coordinates the visible window starts past the baked padding
				var scrollBounds = new Rect(ScrollX + contentInsets.Left, ScrollY + contentInsets.Top, viewportWidth, viewportHeight);
				var itemBounds = new Rect(x, y, item.Width, item.Height);
				if (scrollBounds.Contains(itemBounds))
					return new Point(ScrollX, ScrollY);
				switch (Orientation)
				{
					case ScrollOrientation.Vertical:
						position = y > scrollBounds.Y ? ScrollToPosition.End : ScrollToPosition.Start;
						break;
					case ScrollOrientation.Horizontal:
						position = x > scrollBounds.X ? ScrollToPosition.End : ScrollToPosition.Start;
						break;
					case ScrollOrientation.Both:
						position = x > scrollBounds.X || y > scrollBounds.Y ? ScrollToPosition.End : ScrollToPosition.Start;
						break;
				}
			}
			switch (position)
			{
				case ScrollToPosition.Center:
					y = y - viewportHeight / 2 + item.Height / 2;
					x = x - viewportWidth / 2 + item.Width / 2;
					break;
				case ScrollToPosition.End:
					y = y - viewportHeight + item.Height;
					x = x - viewportWidth + item.Width;
					break;
			}
			return new Point(x - contentInsets.Left, y - contentInsets.Top);
		}

		// The scrollable viewport can be smaller than the frame: on iOS the adjusted content
		// insets obscure part of it. The handler owns that coordinate convention and reports
		// it here; handlers whose viewport always equals the frame don't implement the contract.
		Thickness GetVisibleViewportInsets() =>
			(Handler as IScrollViewportProvider)?.ViewportInsets ?? default;

		Thickness GetContentCoordinateInsets() =>
			(Handler as IScrollViewportProvider)?.ContentCoordinateInsets ?? default;

		/// <summary>
		/// Sends the scroll finished notification.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void SendScrollFinished()
		{
			_scrollCompletionSource?.TrySetResult(true);
		}

		/// <summary>
		/// Sets the scrolled position.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the content of the scroll view.
		/// </summary>
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

			// The content has been arranged, so an element target can now be resolved: its
			// position is read from the content tree, which is only meaningful once laid out
			DispatchPendingScrollToRequest();
		}

		/// <summary>
		/// Gets the size of the scrollable content.
		/// </summary>
		public Size ContentSize
		{
			get { return (Size)GetValue(ContentSizeProperty); }
			private set { SetValue(ContentSizePropertyKey, value); }
		}

		/// <summary>
		/// Gets or sets the scroll orientation.
		/// </summary>
		public ScrollOrientation Orientation
		{
			get { return (ScrollOrientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		/// <summary>
		/// Gets the current horizontal scroll position.
		/// </summary>
		public double ScrollX
		{
			get { return (double)GetValue(ScrollXProperty); }
			private set { SetValue(ScrollXPropertyKey, value); }
		}

		/// <summary>
		/// Gets the current vertical scroll position.
		/// </summary>
		public double ScrollY
		{
			get { return (double)GetValue(ScrollYProperty); }
			private set { SetValue(ScrollYPropertyKey, value); }
		}

		/// <summary>
		/// Gets or sets the horizontal scroll bar visibility.
		/// </summary>
		public ScrollBarVisibility HorizontalScrollBarVisibility
		{
			get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
			set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
		}

		/// <summary>
		/// Gets or sets the vertical scroll bar visibility.
		/// </summary>
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
		/// Use SafeAreaEdges.None for edge-to-edge content, SafeAreaEdges.All to obey all safe area insets, 
		/// SafeAreaEdges.Container for content that flows under keyboard but stays out of bars/notch, or SafeAreaEdges.SoftInput for keyboard-aware behavior.
		/// </remarks>
		public SafeAreaEdges SafeAreaEdges
		{
			get => (SafeAreaEdges)GetValue(SafeAreaElement.SafeAreaEdgesProperty);
			set => SetValue(SafeAreaElement.SafeAreaEdgesProperty, value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScrollView"/> class.
		/// </summary>
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

		/// <summary>
		/// Scrolls to the specified position asynchronously.
		/// </summary>
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

		/// <summary>
		/// Scrolls to the specified element asynchronously.
		/// </summary>
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
			// The task can be completed from inside a lifecycle mutation (a handler change, the
			// view leaving the tree) as well as from platform scroll callbacks. The caller's
			// await continuation must never resume on that stack — it could re-enter a
			// half-finished parenting or property change — so continuations always run
			// asynchronously. The task itself still transitions to completed synchronously.
			_scrollCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
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
				_replayPendingScrollToRequestedEvent = true;
			}
			else if (e.Mode == ScrollToMode.Element && !IsElementTargetGeometryReady())
			{
				// The handler exists but layout has not run yet (e.g. ScrollToAsync from
				// OnAppearing): resolving the element target now would compute against the -1
				// never-arranged sentinels. Park it for the layout callbacks instead — the
				// subscribers were already notified above, so the replay must not re-raise.
				// It stays parked until the arrange arrives or the view's lifecycle ends
				// (see OnHandlerChangedCore); a hidden view scrolls once it is shown.
				_pendingScrollToRequested = e;
				_replayPendingScrollToRequestedEvent = false;
			}
			else
			{
				// This request supersedes anything still queued: a deferred element request
				// whose dispatch is already scheduled must not run afterwards and restore the
				// older target (latest request wins).
				_pendingScrollToRequested = null;

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

		void IScrollOffsetReceiver.UpdateScrollOffsets(double horizontalOffset, double verticalOffset)
		{
			// The reported offsets moved because the platform insets did, not because anything
			// scrolled: keep ScrollX/ScrollY (and their bindings) current without raising Scrolled
			ScrollX = horizontalOffset;
			ScrollY = verticalOffset;
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

			// Geometry is now known, so an element-mode request held back at handler-attach
			// can be resolved
			DispatchPendingScrollToRequest();
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

		/// <inheritdoc cref="ISafeAreaView2.HasExplicitSafeAreaEdges"/>
		bool ISafeAreaView2.HasExplicitSafeAreaEdges => IsSetExplicitly(SafeAreaEdgesProperty);

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