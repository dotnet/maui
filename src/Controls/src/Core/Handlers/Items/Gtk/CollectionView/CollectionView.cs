using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Gdk;
using Gtk;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using CollectionViewSelectionMode = Microsoft.Maui.Controls.SelectionMode;

namespace Gtk.UIExtensions.NUI
{

	/// <summary>
	/// A View that contain a templated list of items.
	/// </summary>
	public partial class CollectionView : Gtk.ScrolledWindow, ICollectionViewController
	{

		double _previousHorizontalOffset;
		double _previousVerticalOffset;

		Widget? _headerView;
		Widget? _footerView;

		Microsoft.Maui.Controls.SelectionMode _selectionMode;

		/// <summary>
		/// Initializes a new instance of the <see cref="CollectionView"/> class.
		/// </summary>
#pragma warning disable CS8618
		// dotnet compiler does not track a method that called on constructor to check non-nullable object
		// https://github.com/dotnet/roslyn/issues/32358
		public CollectionView()
#pragma warning restore CS8618
		{
			// HasWindow = false;
			InitializationComponent();
		}

		/// <summary>
		/// Event that is raised after a scroll completes.
		/// </summary>
		public event EventHandler<CollectionViewScrolledEventArgs>? Scrolled;

		/// <summary>
		/// Gets a ScrollView instance that used in CollectionView
		/// </summary>
		public ScrollableBase ScrollView { get; private set; }

		protected CollectionViewController CollectionViewController { get; set; }

		protected CollectionContainer CollectionContainer { get; set; }

		public ItemAdaptor? Adaptor
		{
			get => CollectionViewController.Adaptor;
			set
			{
				CollectionViewController.Adaptor = value;
				CollectionContainer.Adaptor = value;
			}

		}

		public ICollectionViewLayoutManager? LayoutManager
		{
			get => CollectionViewController.LayoutManager;
			set
			{
				CollectionViewController.LayoutManager = value;
				CollectionContainer.LayoutManager = value;
			}

		}

		/// <summary>
		/// Gets or sets a value that controls whether and how many items can be selected.
		/// </summary>
		public Microsoft.Maui.Controls.SelectionMode SelectionMode
		{
			get => _selectionMode;
			set
			{
				_selectionMode = value;
				CollectionViewController.SelectionMode = value;
			}
		}

		/// <summary>
		/// Specifies the behavior of snap points when scrolling.
		/// </summary>
		public SnapPointsType SnapPointsType { get; set; }

		/// <summary>
		/// Specifies how snap points are aligned with items.
		/// </summary>
		public SnapPointsAlignment SnapPointsAlignment { get; set; }

		/// <summary>
		/// The size of the area that layout in advance before it is visible
		/// </summary>
		public float RedundancyLayoutBoundRatio
		{
			get => CollectionViewController.RedundancyLayoutBoundRatio;
			set => CollectionViewController.RedundancyLayoutBoundRatio = value;
		}

		/// <summary>
		/// A size of allocated by Layout, it become viewport size on scrolling
		/// </summary>
		protected Size AllocatedSize
		{
			get => CollectionViewController.AllocatedSize;
		}

		internal Rect ViewPort => ScrollView.GetScrollBound();

		/// <summary>
		/// Scrolls the CollectionView to the index
		/// </summary>
		/// <param name="index">Index of item</param>
		/// <param name="position">How the item should be positioned on screen.</param>
		/// <param name="animate">Whether or not the scroll should be animated.</param>
		public void ScrollTo(int index, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			if (LayoutManager == null)
				throw new InvalidOperationException("No Layout manager");

			var itemBound = LayoutManager.GetItemBound(index);
			double itemStart;
			double itemEnd;
			double scrollStart;
			double scrollEnd;
			double itemPadding = 0;
			double itemSize;
			double viewportSize;
			var isHorizontal = LayoutManager.IsHorizontal;

			if (isHorizontal)
			{
				itemStart = itemBound.Left;
				itemEnd = itemBound.Right;
				itemSize = itemBound.Width;
				scrollStart = ViewPort.Left;
				scrollEnd = ViewPort.Right;
				viewportSize = ViewPort.Width;
			}
			else
			{
				itemStart = itemBound.Top;
				itemEnd = itemBound.Bottom;
				itemSize = itemBound.Height;
				scrollStart = ViewPort.Top;
				scrollEnd = ViewPort.Bottom;
				viewportSize = ViewPort.Height;
			}

			if (position == ScrollToPosition.MakeVisible)
			{
				if (itemStart < scrollStart)
				{
					position = ScrollToPosition.Start;
				}
				else if (itemEnd > scrollEnd)
				{
					position = ScrollToPosition.End;
				}
				else
				{
					// already visible
					return;
				}
			}

			if (itemSize < viewportSize)
			{
				switch (position)
				{
					case ScrollToPosition.Center:
						itemPadding = (viewportSize - itemSize) / 2;

						break;
					case ScrollToPosition.End:
						itemPadding = (viewportSize - itemSize);

						break;
				}
			}

			if (isHorizontal)
			{
				itemBound.X -= itemPadding;
			}
			else
			{
				itemBound.Y -= itemPadding;
			}

			ScrollView.ScrollTo(isHorizontal ? (float)itemBound.X : (float)itemBound.Y, animate);
		}

		/// <summary>
		/// Scrolls the CollectionView to the item
		/// </summary>
		/// <param name="item">Item to scroll</param>
		/// <param name="position">How the item should be positioned on screen.</param>
		/// <param name="animate">Whether or not the scroll should be animated.</param>
		public void ScrollTo(object item, ScrollToPosition position = ScrollToPosition.MakeVisible, bool animate = true)
		{
			if (Adaptor == null)
				throw new InvalidOperationException("No Adaptor");

			ScrollTo(Adaptor.GetItemIndex(item), position, animate);
		}

		/// <summary>
		/// Create a ScrollView to use in CollectionView
		/// </summary>
		/// <returns>A ScrollView instance</returns>
		protected virtual ScrollableBase CreateScrollView()
		{
			return new SnappableScrollView(this)
			{
				UseCostomScrolling = true,
				MaximumVelocity = 8.5f,
				Friction = 0.015f
			};
		}

		public IView? VirtualView
		{
			get => CollectionContainer.VirtualView;
			set => CollectionContainer.VirtualView = value;
		}

		/// <summary>
		/// Initialize internal components, such as ScrollView
		/// </summary>
		protected virtual void InitializationComponent()
		{

			CollectionViewController = new CollectionViewController()
			{
				SelectionMode = this.SelectionMode,
				GetViewPort = () => ViewPort,
				AddToContainer = holder => CollectionContainer.AddItem(holder),
				RemoveFromContainer = holder => CollectionContainer.RemoveItem(holder),
				ScrollTo = args => ScrollTo(args.index, args.position, args.animate),

			};

			CollectionViewController.HasContentSizeUpdated += (sender, size) =>
			{
				if (CollectionContainer.IsSizeAllocating || CollectionContainer.IsMeasuring || CollectionContainer.IsReallocating) return;

				CollectionContainer.UpdateSize(size);

				if (!CollectionContainer.Visible && !size.IsZero)
				{
					CollectionContainer.SizeAllocate(new(0, 0, (int)size.Width, (int)size.Height));
					CollectionContainer.Show();
				}
			};

			CollectionViewController.UpdateHeaderFooter += (sender, args) => UpdateHeaderFooter();

			CollectionViewController.AdaptorChanging += (sender, args) =>
			{
				// reset header view
				if (_headerView != null)
				{
					_headerView.Unparent();
					Adaptor?.RemoveHeaderView(_headerView);
					_headerView = null;
				}

				// reset footer view
				if (_footerView != null)
				{
					_footerView.Unparent();
					Adaptor?.RemoveFooterView(_footerView);
					_footerView.Dispose();
					_footerView = null;
				}
			};

			CollectionViewController.AdaptorChanged += (sender, args) =>
			{
				if (Adaptor is not { })
					return;

				_headerView = Adaptor.GetHeaderView();

				if (_headerView != null)
				{
					ScrollView.ContentContainer.Add(_headerView);
				}

				_footerView = Adaptor.GetFooterView();

				if (_footerView != null)
				{
					ScrollView.ContentContainer.Add(_footerView);
				}

				UpdateHeaderFooter();
			};

			CollectionContainer = new CollectionContainer()
			{
				// VirtualView = VirtualView,
				Adaptor = Adaptor,
				LayoutManager = LayoutManager
			};

			this.Child = CollectionContainer;

			this.WidthSpecification(LayoutParamPolicies.MatchParent);
			this.HeightSpecification(LayoutParamPolicies.MatchParent);

			ScrollView = CreateScrollView();
			ScrollView.WidthSpecification(LayoutParamPolicies.MatchParent);
			ScrollView.HeightSpecification(LayoutParamPolicies.MatchParent);
			ScrollView.WidthResizePolicy(ResizePolicyType.FillToParent);
			ScrollView.HeightResizePolicy(ResizePolicyType.FillToParent);

			ScrollView.ScrollingEventThreshold = 10;
			ScrollView.Scrolling += OnScrolling;
			ScrollView.ScrollAnimationEnded += OnScrollAnimationEnded;

			SetPolicy(PolicyType.Automatic, PolicyType.Automatic);

			if (ScrollView is SnappableScrollView snappable)
			{
				snappable.SnapRequestFinished += OnSnapRequestFinished;
			}

			ScrollView.Relayout += CollectionViewController.OnLayout;

			// Add(ScrollView);
		}

		protected override void OnSizeAllocated(Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);
			ShowAll();
		}

		void OnScrollAnimationEnded(object? sender, ScrollEventArgs e)
		{
			SendScrolledEvent();
		}

		void OnSnapRequestFinished(object? sender, EventArgs e)
		{
			SendScrolledEvent();
		}

		void OnScrolling(object? sender, ScrollEventArgs e)
		{
			if (LayoutManager == null)
				return;

			if (sender is IScrollable sa)
			{
				;
			}

			var viewport = ViewPort;
			var viewportFromEvent = new Rect(-e.Event.X, -e.Event.Y, viewport.Width, viewport.Height);

			CollectionViewController.LayoutManager?.LayoutItems(ExtendViewPort(viewportFromEvent));
		}

		void SendScrolledEvent()
		{
			if (LayoutManager == null)
				return;

			var args = new CollectionViewScrolledEventArgs();
			args.FirstVisibleItemIndex = LayoutManager.GetVisibleItemIndex(ViewPort.X, ViewPort.Y);
			args.CenterItemIndex = LayoutManager.GetVisibleItemIndex(ViewPort.X + (ViewPort.Width / 2), ViewPort.Y + (ViewPort.Height / 2));
			args.LastVisibleItemIndex = LayoutManager.GetVisibleItemIndex(ViewPort.X + ViewPort.Width, ViewPort.Y + ViewPort.Height);
			args.HorizontalOffset = ViewPort.X;
			args.HorizontalDelta = ViewPort.X - _previousHorizontalOffset;
			args.VerticalOffset = ViewPort.Y;
			args.VerticalDelta = ViewPort.Y - _previousVerticalOffset;
			Scrolled?.Invoke(this, args);

			_previousHorizontalOffset = ViewPort.X;
			_previousVerticalOffset = ViewPort.Y;
		}

		void UpdateHeaderFooter()
		{
			if (LayoutManager != null)
			{
				double widthConstraint = LayoutManager.IsHorizontal ? double.PositiveInfinity : AllocatedSize.Width;
				double heightConstraint = LayoutManager.IsHorizontal ? AllocatedSize.Height : double.PositiveInfinity;

				LayoutManager.SetHeader(_headerView,
					_headerView != null ? Adaptor!.MeasureHeader(widthConstraint, heightConstraint) : new Size(0, 0));

				LayoutManager.SetFooter(_footerView,
					_footerView != null ? Adaptor!.MeasureFooter(widthConstraint, heightConstraint) : new Size(0, 0));
			}
		}

		Rect ExtendViewPort(Rect viewport)
		{
			if (LayoutManager == null)
				return viewport;

			if (LayoutManager.IsHorizontal)
			{
				viewport.X = Math.Max(0, viewport.X - viewport.Width * RedundancyLayoutBoundRatio / 2f);
				viewport.Width += viewport.Width * RedundancyLayoutBoundRatio;
			}
			else
			{
				viewport.Y = Math.Max(0, viewport.Y - viewport.Height * RedundancyLayoutBoundRatio / 2f);
				viewport.Height += viewport.Height * RedundancyLayoutBoundRatio;
			}

			return viewport;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && ScrollView is IDisposable d)
			{
				d.Dispose();
			}

			base.Dispose(disposing);
		}

		ViewHolder ICollectionViewController.RealizeView(int index) => CollectionViewController.RealizeView(index);

		void ICollectionViewController.UnrealizeView(ViewHolder view) => CollectionViewController.UnrealizeView(view);

		void ICollectionViewController.RequestLayoutItems() => CollectionViewController.RequestLayoutItems();

		public int Count
		{
			get => CollectionViewController.Count;
		}

		Size ICollectionViewController.GetItemSize() => CollectionViewController.GetItemSize();

		Size ICollectionViewController.GetItemSize(double widthConstraint, double heightConstraint) => CollectionViewController.GetItemSize(widthConstraint, heightConstraint);

		Size ICollectionViewController.GetItemSize(int index, double widthConstraint, double heightConstraint) => CollectionViewController.GetItemSize(index, widthConstraint, heightConstraint);

		void ICollectionViewController.ContentSizeUpdated() => CollectionViewController.ContentSizeUpdated();

		void ICollectionViewController.ItemMeasureInvalidated(int index) => CollectionViewController.ItemMeasureInvalidated(index);

		void ICollectionViewController.RequestItemSelect(int index) => CollectionViewController.RequestItemSelect(index);

	}

}