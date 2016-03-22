using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Support.V7.Widget;
using AndroidListView = Android.Widget.ListView;
using static System.Diagnostics.Debug;
using Observer = Android.Support.V7.Widget.RecyclerView.AdapterDataObserver;
using BclDebug = System.Diagnostics.Debug;
using IntRectangle = System.Drawing.Rectangle;
using IntSize = System.Drawing.Size;

namespace Xamarin.Forms.Platform.Android
{
	public class CarouselViewRenderer : ViewRenderer<CarouselView, RecyclerView>
	{
		PhysicalLayoutManager _physicalLayout;
		int _position;

		public CarouselViewRenderer()
		{
			AutoPackage = false;
		}

		ItemViewAdapter Adapter
		{
			get { return (ItemViewAdapter)Control.GetAdapter(); }
		}

		new RecyclerView Control
		{
			get
			{
				Initialize();
				return base.Control;
			}
		}

		ICarouselViewController Controller => Element;

		PhysicalLayoutManager LayoutManager
		{
			get { return (PhysicalLayoutManager)Control.GetLayoutManager(); }
		}

		protected override Size MinimumSize()
		{
			return new Size(40, 40);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			CarouselView oldElement = e.OldElement;
			if (oldElement != null)
				e.OldElement.CollectionChanged -= OnCollectionChanged;

			base.OnElementChanged(e);
			Initialize();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Position" && _position != Element.Position)
				_physicalLayout.ScrollToPosition(Element.Position);

			base.OnElementPropertyChanged(sender, e);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			int width = right - left;
			int height = bottom - top;

			LayoutManager.Layout(width, height);

			base.OnLayout(changed, left, top, right, bottom);

			Control.Measure(new MeasureSpecification(width, MeasureSpecificationType.Exactly), new MeasureSpecification(height, MeasureSpecificationType.Exactly));

			Control.Layout(0, 0, width, height);
		}

		void Initialize()
		{
			// cache hit? Check if the view page is already created
			RecyclerView recyclerView = base.Control;
			if (recyclerView != null)
				return;

			// cache miss
			recyclerView = new RecyclerView(Context);
			SetNativeControl(recyclerView);

			// layoutManager
			recyclerView.SetLayoutManager(_physicalLayout = new PhysicalLayoutManager(Context, new VirtualLayoutManager(), Element.Position));

			// swiping
			var dragging = false;
			recyclerView.AddOnScrollListener(new OnScrollListener(onDragStart: () => dragging = true, onDragEnd: () =>
			{
				dragging = false;
				IntVector velocity = _physicalLayout.Velocity;

				int target = velocity.X > 0 ? _physicalLayout.VisiblePositions().Max() : _physicalLayout.VisiblePositions().Min();
				_physicalLayout.ScrollToPosition(target);
			}));

			// scrolling
			var scrolling = false;
			_physicalLayout.OnBeginScroll += position => scrolling = true;
			_physicalLayout.OnEndScroll += position => scrolling = false;

			// appearing
			_physicalLayout.OnAppearing += appearingPosition => { Controller.SendPositionAppearing(appearingPosition); };

			// disappearing
			_physicalLayout.OnDisappearing += disappearingPosition =>
			{
				Controller.SendPositionDisappearing(disappearingPosition);

				// animation completed
				if (!scrolling && !dragging)
				{
					_position = _physicalLayout.VisiblePositions().Single();

					OnPositionChanged();
					OnItemChanged();
				}
			};

			// adapter
			var adapter = new ItemViewAdapter(this);
			adapter.RegisterAdapterDataObserver(new PositionUpdater(this));
			recyclerView.SetAdapter(adapter);

			// initialize properties
			Element.Position = 0;

			// initialize events
			Element.CollectionChanged += OnCollectionChanged;
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Adapter.NotifyItemRangeInserted(e.NewStartingIndex, e.NewItems.Count);
					break;

				case NotifyCollectionChangedAction.Move:
					for (var i = 0; i < e.NewItems.Count; i++)
						Adapter.NotifyItemMoved(e.OldStartingIndex + i, e.NewStartingIndex + i);
					break;

				case NotifyCollectionChangedAction.Remove:
					if (Element.Count == 0)
						throw new InvalidOperationException("CarouselView must retain a least one item.");

					Adapter.NotifyItemRangeRemoved(e.OldStartingIndex, e.OldItems.Count);
					break;

				case NotifyCollectionChangedAction.Replace:
					Adapter.NotifyItemRangeChanged(e.OldStartingIndex, e.OldItems.Count);
					break;

				case NotifyCollectionChangedAction.Reset:
					Adapter.NotifyDataSetChanged();
					break;

				default:
					throw new Exception($"Enum value '{(int)e.Action}' is not a member of NotifyCollectionChangedAction enumeration.");
			}
		}

		void OnItemChanged()
		{
			object item = ((IItemViewController)Element).GetItem(_position);
			Controller.SendSelectedItemChanged(item);
		}

		void OnPositionChanged()
		{
			Element.Position = _position;
			Controller.SendSelectedPositionChanged(_position);
		}

		// http://developer.android.com/reference/android/support/v7/widget/RecyclerView.html
		// http://developer.android.com/training/material/lists-cards.html
		// http://wiresareobsolete.com/2014/09/building-a-recyclerview-layoutmanager-part-1/

		class OnScrollListener : RecyclerView.OnScrollListener
		{
			readonly Action _onDragEnd;
			readonly Action _onDragStart;
			ScrollState _lastScrollState;

			internal OnScrollListener(Action onDragEnd, Action onDragStart)
			{
				_onDragEnd = onDragEnd;
				_onDragStart = onDragStart;
			}

			public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
			{
				var state = (ScrollState)newState;
				if (_lastScrollState != ScrollState.Dragging && state == ScrollState.Dragging)
					_onDragStart();

				if (_lastScrollState == ScrollState.Dragging && state != ScrollState.Dragging)
					_onDragEnd();

				_lastScrollState = state;
				base.OnScrollStateChanged(recyclerView, newState);
			}

			enum ScrollState
			{
				Idle,
				Dragging,
				Settling
			}
		}

		class PositionUpdater : Observer
		{
			readonly CarouselViewRenderer _carouselView;

			internal PositionUpdater(CarouselViewRenderer carouselView)
			{
				_carouselView = carouselView;
			}

			public override void OnItemRangeInserted(int positionStart, int itemCount)
			{
				// removal after the current position won't change current position
				if (positionStart > _carouselView._position)
					;

				// raise position changed
				else
				{
					_carouselView._position += itemCount;
					_carouselView.OnPositionChanged();
				}

				base.OnItemRangeInserted(positionStart, itemCount);
			}

			public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
			{
				base.OnItemRangeMoved(fromPosition, toPosition, itemCount);
			}

			public override void OnItemRangeRemoved(int positionStart, int itemCount)
			{
				Assert(itemCount == 1);

				// removal after the current position won't change current position
				if (positionStart > _carouselView._position)
					;

				// raise item changed
				else if (positionStart == _carouselView._position && positionStart != _carouselView.Adapter.ItemCount)
				{
					_carouselView.OnItemChanged();
					return;
				}

				// raise position changed
				else
				{
					_carouselView._position -= itemCount;
					_carouselView.OnPositionChanged();
				}

				base.OnItemRangeRemoved(positionStart, itemCount);
			}
		}

		internal class VirtualLayoutManager : PhysicalLayoutManager.VirtualLayoutManager
		{
			const int Columns = 1;

			IntSize _itemSize;

			internal override bool CanScrollHorizontally => true;

			internal override bool CanScrollVertically => false;

			public override string ToString()
			{
				return $"itemSize={_itemSize}";
			}

			internal override IntRectangle GetBounds(int originPosition, RecyclerView.State state)
				=> new IntRectangle(LayoutItem(originPosition, 0).Location, new IntSize(_itemSize.Width * state.ItemCount, _itemSize.Height));

			internal override Tuple<int, int> GetPositions(int positionOrigin, int itemCount, IntRectangle viewport, bool includeBuffer)
			{
				// returns one item off-screen in either direction. 
				int buffer = includeBuffer ? 1 : 0;
				int left = GetPosition(itemCount, positionOrigin - buffer, viewport.Left);
				int right = GetPosition(itemCount, positionOrigin + buffer, viewport.Right, true);

				int start = left;
				int count = right - left + 1;
				return new Tuple<int, int>(start, count);
			}

			internal override void Layout(int positionOffset, IntSize viewportSize, ref IntVector offset)
			{
				int width = viewportSize.Width / Columns;
				int height = viewportSize.Height;

				if (_itemSize.Width != 0)
					offset *= (double)width / _itemSize.Width;

				_itemSize = new IntSize(width, height);
			}

			internal override IntRectangle LayoutItem(int positionOffset, int position)
			{
				// measure
				IntSize size = _itemSize;

				// layout
				var location = new IntVector((position - positionOffset) * size.Width, 0);

				// allocate
				return new IntRectangle(location, size);
			}

			int GetPosition(int itemCount, int positionOrigin, int x, bool exclusive = false)
			{
				int position = x / _itemSize.Width + positionOrigin;
				bool hasRemainder = x % _itemSize.Width != 0;

				if (hasRemainder && x < 0)
					position--;

				if (!hasRemainder && exclusive)
					position--;

				position = position.Clamp(0, itemCount - 1);
				return position;
			}
		}
	}

	// RecyclerView virtualizes indexes (adapter position <-> viewGroup child index) 
	// PhysicalLayoutManager virtualizes location (regular layout <-> screen)
}