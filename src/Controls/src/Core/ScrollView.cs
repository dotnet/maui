using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="Type[@FullName='Microsoft.Maui.Controls.ScrollView']/Docs/*" />
	[ContentProperty(nameof(Content))]
	public partial class ScrollView : Compatibility.Layout, IScrollViewController, IElementConfiguration<ScrollView>, IFlowDirectionController
	{
		#region IScrollViewController

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='LayoutAreaOverride']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Rect LayoutAreaOverride
		{
			get => _layoutAreaOverride;
			set
			{
				if (_layoutAreaOverride == value)
					return;
				_layoutAreaOverride = value;
				// Dont invalidate here, we can relayout immediately since this only impacts our innards
				UpdateChildrenLayout();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

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
			if (_scrollCompletionSource != null)
				_scrollCompletionSource.TrySetResult(true);
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

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='OrientationProperty']/Docs/*" />
		public static readonly BindableProperty OrientationProperty = BindableProperty.Create("Orientation", typeof(ScrollOrientation), typeof(ScrollView), ScrollOrientation.Vertical);

		static readonly BindablePropertyKey ScrollXPropertyKey = BindableProperty.CreateReadOnly("ScrollX", typeof(double), typeof(ScrollView), 0d);

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ScrollXProperty']/Docs/*" />
		public static readonly BindableProperty ScrollXProperty = ScrollXPropertyKey.BindableProperty;

		static readonly BindablePropertyKey ScrollYPropertyKey = BindableProperty.CreateReadOnly("ScrollY", typeof(double), typeof(ScrollView), 0d);

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ScrollYProperty']/Docs/*" />
		public static readonly BindableProperty ScrollYProperty = ScrollYPropertyKey.BindableProperty;

		static readonly BindablePropertyKey ContentSizePropertyKey = BindableProperty.CreateReadOnly("ContentSize", typeof(Size), typeof(ScrollView), default(Size));

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ContentSizeProperty']/Docs/*" />
		public static readonly BindableProperty ContentSizeProperty = ContentSizePropertyKey.BindableProperty;

		readonly Lazy<PlatformConfigurationRegistry<ScrollView>> _platformConfigurationRegistry;

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='HorizontalScrollBarVisibilityProperty']/Docs/*" />
		public static readonly BindableProperty HorizontalScrollBarVisibilityProperty = BindableProperty.Create(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ScrollView), ScrollBarVisibility.Default);

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='VerticalScrollBarVisibilityProperty']/Docs/*" />
		public static readonly BindableProperty VerticalScrollBarVisibilityProperty = BindableProperty.Create(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(ScrollView), ScrollBarVisibility.Default);

		View _content;
		TaskCompletionSource<bool> _scrollCompletionSource;
		Rect _layoutAreaOverride;

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='Content']/Docs/*" />
		public View Content
		{
			get { return _content; }
			set
			{
				if (_content == value)
					return;

				OnPropertyChanging();
				if (_content != null)
				{
					_content.SizeChanged -= ContentSizeChanged;
					InternalChildren.Remove(_content);
				}
				_content = value;
				if (_content != null)
				{
					InternalChildren.Add(_content);
					_content.SizeChanged += ContentSizeChanged;
				}

				OnPropertyChanged();
				Handler?.UpdateValue(nameof(Content));
			}
		}

		void ContentSizeChanged(object sender, EventArgs e)
		{
			var view = (sender as IView);
			if (view == null)
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
				return Task.FromResult(false);

			var args = new ScrollToRequestedEventArgs(x, y, animated);
			OnScrollToRequested(args);
			return _scrollCompletionSource.Task;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrollView.xml" path="//Member[@MemberName='ScrollToAsync'][2]/Docs/*" />
		public Task ScrollToAsync(Element element, ScrollToPosition position, bool animated)
		{
			if (Orientation == ScrollOrientation.Neither)
				return Task.FromResult(false);

			if (!Enum.IsDefined(typeof(ScrollToPosition), position))
				throw new ArgumentException("position is not a valid ScrollToPosition", "position");

			if (element == null)
				throw new ArgumentNullException("element");

			if (!CheckElementBelongsToScrollViewer(element))
				throw new ArgumentException("element does not belong to this ScrollView", "element");

			var args = new ScrollToRequestedEventArgs(element, position, animated);
			OnScrollToRequested(args);
			return _scrollCompletionSource.Task;
		}

		bool IFlowDirectionController.ApplyEffectiveFlowDirectionToChildContainer => false;

		protected override void LayoutChildren(double x, double y, double width, double height)
		{
		}

		protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (Content == null)
				return new SizeRequest();

			switch (Orientation)
			{
				case ScrollOrientation.Horizontal:
					widthConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Vertical:
					heightConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Both:
					widthConstraint = double.PositiveInfinity;
					heightConstraint = double.PositiveInfinity;
					break;
				case ScrollOrientation.Neither:
					widthConstraint = Width;
					heightConstraint = Height;
					break;
			}

			SizeRequest contentRequest;

			if (Content is IView fe && fe.Handler != null)
			{
				contentRequest = fe.Measure(widthConstraint, heightConstraint);
			}
			else
			{
				contentRequest = Content.Measure(widthConstraint, heightConstraint, MeasureFlags.IncludeMargins);
			}

			contentRequest.Minimum = new Size(Math.Min(40, contentRequest.Minimum.Width), Math.Min(40, contentRequest.Minimum.Height));

			return contentRequest;
		}

		internal override void ComputeConstraintForView(View view)
		{
			switch (Orientation)
			{
				case ScrollOrientation.Horizontal:
					LayoutOptions vOptions = view.VerticalOptions;
					if (vOptions.Alignment == LayoutAlignment.Fill && (Constraint & LayoutConstraint.VerticallyFixed) != 0)
					{
						view.ComputedConstraint = LayoutConstraint.VerticallyFixed;
					}
					break;
				case ScrollOrientation.Vertical:
					LayoutOptions hOptions = view.HorizontalOptions;
					if (hOptions.Alignment == LayoutAlignment.Fill && (Constraint & LayoutConstraint.HorizontallyFixed) != 0)
					{
						view.ComputedConstraint = LayoutConstraint.HorizontallyFixed;
					}
					break;
				case ScrollOrientation.Both:
					view.ComputedConstraint = LayoutConstraint.None;
					break;
			}
		}

		bool CheckElementBelongsToScrollViewer(Element element)
		{
			return Equals(element, this) || element.RealParent != null && CheckElementBelongsToScrollViewer(element.RealParent);
		}

		void CheckTaskCompletionSource()
		{
			if (_scrollCompletionSource != null && _scrollCompletionSource.Task.Status == TaskStatus.Running)
			{
				_scrollCompletionSource.TrySetCanceled();
			}
			_scrollCompletionSource = new TaskCompletionSource<bool>();
		}

		double GetCoordinate(Element item, string coordinateName, double coordinate)
		{
			if (item == this)
				return coordinate;
			coordinate += (double)typeof(VisualElement).GetProperty(coordinateName).GetValue(item, null);
			var visualParentElement = item.RealParent as VisualElement;
			return visualParentElement != null ? GetCoordinate(visualParentElement, coordinateName, coordinate) : coordinate;
		}

		double GetMaxHeight(double height)
		{
			return Math.Max(height, _content.Bounds.Top + Padding.Top + _content.Bounds.Bottom + Padding.Bottom);
		}

		static double GetMaxHeight(double height, SizeRequest size)
		{
			return Math.Max(size.Request.Height, height);
		}

		double GetMaxWidth(double width)
		{
			return Math.Max(width, _content.Bounds.Left + Padding.Left + _content.Bounds.Right + Padding.Right);
		}

		static double GetMaxWidth(double width, SizeRequest size)
		{
			return Math.Max(size.Request.Width, width);
		}

		void OnScrollToRequested(ScrollToRequestedEventArgs e)
		{
			CheckTaskCompletionSource();
			ScrollToRequested?.Invoke(this, e);

			Handler?.Invoke(nameof(IScrollView.RequestScrollTo), ConvertRequestMode(e).ToRequest());
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
	}
}
