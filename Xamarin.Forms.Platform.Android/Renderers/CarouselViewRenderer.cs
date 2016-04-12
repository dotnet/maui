using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Support.V7.Widget;
using static System.Diagnostics.Debug;
using IntRectangle = System.Drawing.Rectangle;
using IntSize = System.Drawing.Size;
using IntPoint = System.Drawing.Point;
using AndroidView = Android.Views.View;
using Adapter = Android.Support.V7.Widget.RecyclerView.Adapter;
using Recycler = Android.Support.V7.Widget.RecyclerView.Recycler;
using State = Android.Support.V7.Widget.RecyclerView.State;
using ViewHolder = Android.Support.V7.Widget.RecyclerView.ViewHolder;
using Observer = Android.Support.V7.Widget.RecyclerView.AdapterDataObserver;
using LayoutManager = Android.Support.V7.Widget.RecyclerView.LayoutManager;
using LayoutParams = Android.Support.V7.Widget.RecyclerView.LayoutParams;

namespace Xamarin.Forms.Platform.Android
{
	internal static class CarouselViewExtensions
	{
		internal static IntVector BoundTranslation(this IntRectangle viewport, IntVector delta, IntRectangle bound)
		{
			// TODO: generalize the math
			Assert(delta.X == 0 || delta.Y == 0);

			IntVector start = viewport.LeadingCorner(delta);
			IntVector end = start + delta;
			IntVector clampedEnd = end.Clamp(bound);
			IntVector clampedDelta = clampedEnd - start;
			return clampedDelta;
		}
		internal static IntVector Clamp(this IntVector position, IntRectangle bound)
		{
			return new IntVector(
				x: position.X.Clamp(bound.Left, bound.Right), 
				y: position.Y.Clamp(bound.Top, bound.Bottom)
			);
		}
		internal static IntVector LeadingCorner(this IntRectangle rectangle, IntVector delta)
		{
			return new IntVector(
				x: delta.X < 0 ? rectangle.Left : rectangle.Right, 
				y: delta.Y < 0 ? rectangle.Top : rectangle.Bottom
			);
		}
		internal static IntVector Center(this IntRectangle rectangle)
		{
			return (IntVector)rectangle.Location + (IntVector)rectangle.Size / 2;
		}
		internal static int Area(this IntRectangle rectangle)
		{
			return rectangle.Width * rectangle.Height;
		}

		internal static Rectangle ToFormsRectangle(this IntRectangle rectangle, Context context)
		{
			return new Rectangle(
				x: context.FromPixels(rectangle.Left),
				y: context.FromPixels(rectangle.Top),
				width: context.FromPixels(rectangle.Width), 
				height: context.FromPixels(rectangle.Height)
			);
		}
		internal static Rect ToAndroidRectangle(this IntRectangle rectangle)
		{
			return new Rect(
				left: rectangle.Left, 
				right: rectangle.Right, 
				top: rectangle.Top, 
				bottom: rectangle.Bottom
			);
		}

		internal static bool LexicographicallyLess(this IntPoint source, IntPoint target)
		{
			if (source.X < target.X)
				return true;

			if (source.X > target.X)
				return false;

			return source.Y < target.Y;
		}
		internal static int[] ToRange(this Tuple<int, int> startAndCount)
		{
			return Enumerable.Range(startAndCount.Item1, startAndCount.Item2).ToArray();
		}
	}

	internal struct IntVector
	{
		public static explicit operator IntVector(IntSize size)
		{
			return new IntVector(size.Width, size.Height);
		}
		public static explicit operator IntVector(IntPoint point)
		{
			return new IntVector(point.X, point.Y);
		}
		public static implicit operator IntPoint(IntVector vector)
		{
			return new IntPoint(vector.X, vector.Y);
		}
		public static implicit operator IntSize(IntVector vector)
		{
			return new IntSize(vector.X, vector.Y);
		}

		public static bool operator ==(IntVector lhs, IntVector rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}
		public static bool operator !=(IntVector lhs, IntVector rhs)
		{
			return !(lhs == rhs);
		}
		public static IntRectangle operator -(IntRectangle source, IntVector vector) => source + -vector;
		public static IntRectangle operator +(IntRectangle source, IntVector vector) => 
			new IntRectangle(source.Location + vector, source.Size);

		public static IntVector operator -(IntVector vector, IntVector other) => vector + -other;
		public static IntVector operator +(IntVector vector, IntVector other) => 
			new IntVector(
				x: vector.X + other.X, 
				y: vector.Y + other.Y
			);

		public static IntPoint operator -(IntPoint point, IntVector delta) => point + -delta;
		public static IntPoint operator +(IntPoint point, IntVector delta) => 
			new IntPoint(
				x: point.X + delta.X, 
				y: point.Y + delta.Y
			);

		public static IntVector operator -(IntVector vector) => vector * -1;
		public static IntVector operator *(IntVector vector, int scaler) => 
			new IntVector(
				x: vector.X * scaler, 
				y: vector.Y * scaler
			);
		public static IntVector operator /(IntVector vector, int scaler) => 
			new IntVector(
				x: vector.X / scaler, 
				y: vector.Y / scaler
			);

		public static IntVector operator *(IntVector vector, double scaler) => 
			new IntVector(
				x: (int)(vector.X * scaler),
				y:  (int)(vector.Y * scaler)
			);
		public static IntVector operator /(IntVector vector, double scaler) => vector * (1 / scaler);

		internal static IntVector Origin = new IntVector(0, 0);
		internal static IntVector XUnit = new IntVector(1, 0);
		internal static IntVector YUnit = new IntVector(0, 1);

		#region Fields
		readonly int _x;
		readonly int _y;
		#endregion

		internal IntVector(int x, int y)
		{
			_x = x;
			_y = y;
		}

		internal int X => _x;
		internal int Y => _y;

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override string ToString()
		{
			return $"{X},{Y}";
		}
	}

	[Flags]
	internal enum MeasureSpecificationType
	{
		Unspecified = 0,
		Exactly = 0x1 << 31,
		AtMost = 0x1 << 32,
		Mask = Exactly | AtMost
	}
	internal struct MeasureSpecification
	{
		public static explicit operator MeasureSpecification(int measureSpecification)
		{
			return new MeasureSpecification(measureSpecification);
		}
		public static implicit operator int(MeasureSpecification measureSpecification)
		{
			return measureSpecification.Encode();
		}

		#region Fields
		readonly int _value;
		readonly MeasureSpecificationType _type;
		#endregion

		internal MeasureSpecification(int measureSpecification)
		{
			_value = measureSpecification & (int)~MeasureSpecificationType.Mask;
			_type = (MeasureSpecificationType)(measureSpecification & (int)MeasureSpecificationType.Mask);
		}
		internal MeasureSpecification(int value, MeasureSpecificationType measureSpecification)
		{
			_value = value;
			_type = measureSpecification;
		}

		internal int Value => _value;
		internal MeasureSpecificationType Type => _type;
		internal int Encode() => Value | (int)Type;

		public override string ToString()
		{
			return string.Format("{0} {1}", Value, Type);
		}
	}

	public class CarouselViewRenderer : ViewRenderer<CarouselView, RecyclerView>
	{
		// http://developer.android.com/reference/android/support/v7/widget/RecyclerView.html
		// http://developer.android.com/training/material/lists-cards.html
		// http://wiresareobsolete.com/2014/09/building-a-recyclerview-layoutmanager-part-1/

		internal class VirtualLayoutManager : PhysicalLayoutManager.VirtualLayoutManager
		{
			#region Fields
			const int Columns = 1;
			IntSize _itemSize;
			#endregion

			#region Private Members
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
			#endregion

			internal override bool CanScrollHorizontally => true;
			internal override bool CanScrollVertically => false;

			internal override IntRectangle GetBounds(int originPosition, State state) => 
				new IntRectangle(
					LayoutItem(originPosition, 0).Location, 
					new IntSize(_itemSize.Width * state.ItemCount, _itemSize.Height)
				);

			internal override Tuple<int, int> GetPositions(
				int positionOrigin, 
				int itemCount, 
				IntRectangle viewport, 
				bool includeBuffer)
			{
				// returns one item off-screen in either direction. 
				int buffer = includeBuffer ? 1 : 0;
				int left = GetPosition(itemCount, positionOrigin - buffer, viewport.Left);
				int right = GetPosition(itemCount, positionOrigin + buffer, viewport.Right, exclusive: true);

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

			public override string ToString()
			{
				return $"itemSize={_itemSize}";
			}
		}

		#region Private Definitions
		class OnScrollListener : RecyclerView.OnScrollListener
		{
			enum ScrollState
			{
				Idle,
				Dragging,
				Settling
			}

			readonly Action _onDragEnd;
			readonly Action _onDragStart;
			ScrollState _lastScrollState;

			internal OnScrollListener(
				Action onDragEnd,
				Action onDragStart)
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
		}
		class PositionUpdater : Observer
		{
			#region Fields
			readonly CarouselViewRenderer _carouselView;
			#endregion

			internal PositionUpdater(CarouselViewRenderer carouselView)
			{
				_carouselView = carouselView;
			}

			public override void OnItemRangeInserted(int positionStart, int itemCount)
			{
				if (positionStart > _carouselView._position)
				{
					// removal after the current position won't change current position
				}
				else
				{
					// raise position changed
					_carouselView._position += itemCount;
					_carouselView.OnPositionChanged();
				}

				base.OnItemRangeInserted(positionStart, itemCount);
			}
			public override void OnItemRangeRemoved(int positionStart, int itemCount)
			{
				Assert(itemCount == 1);

				if (positionStart > _carouselView._position)
				{
					// removal after the current position won't change current position
				}
				else if (positionStart == _carouselView._position &&
					positionStart != _carouselView.Adapter.ItemCount)
				{
					// raise item changed
					_carouselView.OnItemChanged();
					return;
				}
				else
				{
					// raise position changed
					_carouselView._position -= itemCount;
					_carouselView.OnPositionChanged();
				}

				base.OnItemRangeRemoved(positionStart, itemCount);
			}
		}
		#endregion

		#region Fields
		PhysicalLayoutManager _physicalLayout;
		int _position;
		bool _disposed;
		#endregion

		public CarouselViewRenderer()
		{
			AutoPackage = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Element != null)
					Element.CollectionChanged -= OnCollectionChanged;
			}

			base.Dispose(disposing);
		}

		#region Private Members
		void Initialize()
		{
			// cache hit? Check if the view page is already created
			RecyclerView recyclerView = Control;
			if (recyclerView != null)
				return;

			// cache miss
			recyclerView = new RecyclerView(Context);
			SetNativeControl(recyclerView);

			// layoutManager
			recyclerView.SetLayoutManager(
				layout: _physicalLayout = new PhysicalLayoutManager(
					context: Context, 
					virtualLayout: new VirtualLayoutManager(), 
					positionOrigin: Element.Position
				)
			);

			// swiping
			var dragging = false;
			recyclerView.AddOnScrollListener(
				new OnScrollListener(
					onDragStart: () => dragging = true, 
					onDragEnd: () => 
					{
						dragging = false;
						var velocity = _physicalLayout.Velocity;

						var target = velocity.X > 0 ? 
							_physicalLayout.VisiblePositions().Max() : 
							_physicalLayout.VisiblePositions().Min();
						_physicalLayout.ScrollToPosition(target);
					}
				)
			);

			// scrolling
			var scrolling = false;
			_physicalLayout.OnBeginScroll += position => scrolling = true;
			_physicalLayout.OnEndScroll += position => scrolling = false;

			// appearing
			_physicalLayout.OnAppearing += appearingPosition => 
			{
				Controller.SendPositionAppearing(appearingPosition);
			};

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
		}

		ItemViewAdapter Adapter => (ItemViewAdapter)Control.GetAdapter();
		PhysicalLayoutManager LayoutManager => (PhysicalLayoutManager)Control.GetLayoutManager();

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					Adapter.NotifyItemRangeInserted(
						positionStart: e.NewStartingIndex, 
						itemCount: e.NewItems.Count
					);
					break;

				case NotifyCollectionChangedAction.Move:
					for (var i = 0; i < e.NewItems.Count; i++)
						Adapter.NotifyItemMoved(
							fromPosition: e.OldStartingIndex + i, 
							toPosition: e.NewStartingIndex + i
						);
					break;

				case NotifyCollectionChangedAction.Remove:
					if (Element.Count == 0)
						throw new InvalidOperationException("CarouselView must retain a least one item.");

					Adapter.NotifyItemRangeRemoved(
						positionStart: e.OldStartingIndex, 
						itemCount: e.OldItems.Count
					);
					break;

				case NotifyCollectionChangedAction.Replace:
					Adapter.NotifyItemRangeChanged(
						positionStart: e.OldStartingIndex, 
						itemCount: e.OldItems.Count
					);
					break;

				case NotifyCollectionChangedAction.Reset:
					Adapter.NotifyDataSetChanged();
					break;

				default:
					throw new Exception($"Enum value '{(int)e.Action}' is not a member of NotifyCollectionChangedAction enumeration.");
			}
		}
		ICarouselViewController Controller => Element;
		IVisualElementController VisualElementController => Element;
		void OnPositionChanged()
		{
			Element.Position = _position;
			Controller.SendSelectedPositionChanged(_position);
		}
		void OnItemChanged()
		{
			object item = ((IItemViewController)Element).GetItem(_position);
			Controller.SendSelectedItemChanged(item);
		}
		#endregion

		protected override void OnElementChanged(ElementChangedEventArgs<CarouselView> e)
		{
			base.OnElementChanged(e);

			CarouselView oldElement = e.OldElement;
			CarouselView newElement = e.NewElement;
			if (oldElement != null)
			{
				e.OldElement.CollectionChanged -= OnCollectionChanged;
			}

			if (newElement != null)
			{
				if (Control == null)
				{
					Initialize();
				}

				// initialize properties
				_position = Element.Position;

				// initialize events
				Element.CollectionChanged += OnCollectionChanged;
			}
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

			Control.Measure(
				widthMeasureSpec: new MeasureSpecification(width, MeasureSpecificationType.Exactly),
				heightMeasureSpec: new MeasureSpecification(height, MeasureSpecificationType.Exactly)
			);

			Control.Layout(0, 0, width, height);
		}
		protected override Size MinimumSize()
		{
			return new Size(40, 40);
		}
	}

	// RecyclerView virtualizes indexes (adapter position <-> viewGroup child index) 
	// PhysicalLayoutManager virtualizes location (regular layout <-> screen)
	internal class PhysicalLayoutManager : LayoutManager
	{
		// ObservableCollection is our public entryway to this method and it only supports single item removal
		internal const int MaxItemsRemoved = 1;

		internal struct DecoratedView
		{
			public static implicit operator AndroidView(DecoratedView view)
			{
				return view._view;
			}

			#region Fields
			readonly PhysicalLayoutManager _layout;
			readonly AndroidView _view;
			#endregion

			internal DecoratedView(
				PhysicalLayoutManager layout, 
				AndroidView view)
			{
				_layout = layout;
				_view = view;
			}

			internal int Left => _layout.GetDecoratedLeft(_view);
			internal int Top => _layout.GetDecoratedTop(_view);
			internal int Bottom => _layout.GetDecoratedBottom(_view);
			internal int Right => _layout.GetDecoratedRight(_view);
			internal int Width => Right - Left;
			internal int Height => Bottom - Top;
			internal IntRectangle Rectangle => new IntRectangle(Left, Top, Width, Height);

			internal void Measure(int widthUsed, int heightUsed)
			{
				_layout.MeasureChild(_view, widthUsed, heightUsed);
			}
			internal void MeasureWithMargins(int widthUsed, int heightUsed)
			{
				_layout.MeasureChildWithMargins(_view, widthUsed, heightUsed);
			}
			internal void Layout(IntRectangle position)
			{
				var renderer = _view as IVisualElementRenderer;
				renderer.Element.Layout(position.ToFormsRectangle(_layout._context));

				// causes the private LAYOUT_REQUIRED flag to be set so we can be sure the Layout call will properly chain through to all children
				Measure(position.Width, position.Height);
				_layout.LayoutDecorated(_view, 
					left: position.Left, 
					top: position.Top, 
					right: position.Right, 
					bottom: position.Bottom
				);
			}
			internal void Add()
			{
				_layout.AddView(_view);
			}
			internal void DetachAndScrap(Recycler recycler)
			{
				_layout.DetachAndScrapView(_view, recycler);
			}
		}
		internal abstract class VirtualLayoutManager
		{
			internal abstract Tuple<int, int> GetPositions(
				int positionOrigin, 
				int itemCount, 
				IntRectangle viewport, 
				bool isPreLayout
			);

			internal abstract IntRectangle LayoutItem(int positionOrigin, int position);
			internal abstract bool CanScrollHorizontally { get; }
			internal abstract bool CanScrollVertically { get; }

			internal abstract void Layout(int positionOrigin, IntSize viewportSize, ref IntVector offset);
			internal abstract IntRectangle GetBounds(int positionOrigin, State state);
		}

		#region Private Defintions
		enum AdapterChangeType
		{
			Removed = 1,
			Added,
			Moved,
			Updated,
			Changed
		}
		enum SnapPreference
		{
			None = 0,
			Begin = 1,
			End = -1
		}
		sealed class SeekAndSnapScroller : LinearSmoothScroller
		{
			#region Fields
			readonly SnapPreference _snapPreference;
			readonly Func<int, IntVector> _vectorToPosition;
			#endregion

			internal SeekAndSnapScroller(
				Context context, 
				Func<int, IntVector> vectorToPosition, 
				SnapPreference snapPreference = SnapPreference.None) 
				: base(context)
			{
				_vectorToPosition = vectorToPosition;
				_snapPreference = snapPreference;
			}

			protected override int HorizontalSnapPreference => (int)_snapPreference;
			protected override void OnStart()
			{
				OnBeginScroll?.Invoke(TargetPosition);
				base.OnStart();
			}
			protected override void OnStop()
			{
				// expected this to be triggered with the animation stops but it
				// actually seems to be triggered when the target is found
				OnEndScroll?.Invoke(TargetPosition);
				base.OnStop();
			}

			public event Action<int> OnBeginScroll;
			public event Action<int> OnEndScroll;

			public override PointF ComputeScrollVectorForPosition(int targetPosition)
			{
				var vector = _vectorToPosition(targetPosition);
				return new PointF(vector.X, vector.Y);
			}

		}
		#endregion

		#region Static Fields
		readonly static int s_samplesCount = 5;
		#endregion

		#region Fields
		readonly Context _context;
		readonly VirtualLayoutManager _virtualLayout;
		readonly Queue<Action<Recycler, State>> _deferredLayout;
		readonly Dictionary<int, AndroidView> _viewByAdaptorPosition;
		readonly HashSet<int> _visibleAdapterPosition;
		readonly SeekAndSnapScroller _scroller;

		int _positionOrigin; // coordinates are relative to the upper left corner of this element
		IntVector _locationOffset; // upper left corner of screen is positionOrigin + locationOffset
		List<IntVector> _samples;
		AdapterChangeType _adapterChangeType;
		#endregion

		internal PhysicalLayoutManager(Context context, VirtualLayoutManager virtualLayout, int positionOrigin)
		{
			_positionOrigin = positionOrigin;
			_context = context;
			_virtualLayout = virtualLayout;
			_viewByAdaptorPosition = new Dictionary<int, AndroidView>();
			_visibleAdapterPosition = new HashSet<int>();
			_samples = Enumerable.Repeat(IntVector.Origin, s_samplesCount).ToList();
			_deferredLayout = new Queue<Action<Recycler, State>>();
			_scroller = new SeekAndSnapScroller(
				context: context, 
				vectorToPosition: adapterPosition => {
					var end = virtualLayout.LayoutItem(_positionOrigin, adapterPosition).Center();
					var begin = Viewport.Center();
					return end - begin;
				}
			);

			_scroller.OnBeginScroll += adapterPosition => OnBeginScroll?.Invoke(adapterPosition);
			_scroller.OnEndScroll += adapterPosition => OnEndScroll?.Invoke(adapterPosition);
		}

		#region Private Members
		// helpers to deal with locations as IntRectangles and IntVectors
		IntRectangle Rectangle => new IntRectangle(0, 0, Width, Height);
		void OffsetChildren(IntVector delta)
		{
			OffsetChildrenHorizontal(-delta.X);
			OffsetChildrenVertical(-delta.Y);
		}
		void ScrollBy(ref IntVector delta, Recycler recycler, State state)
		{
			_adapterChangeType = default(AdapterChangeType);

			delta = Viewport.BoundTranslation(
				delta: delta, 
				bound: _virtualLayout.GetBounds(_positionOrigin, state)
			);

			_locationOffset += delta;
			_samples.Insert(0, delta);
			_samples.RemoveAt(_samples.Count - 1);

			OffsetChildren(delta);
			OnLayoutChildren(recycler, state);
		}
		void OnAppearingOrDisappearing(int position, bool isAppearing)
		{
			if (isAppearing)
			{
				if (!_visibleAdapterPosition.Contains(position))
				{
					_visibleAdapterPosition.Add(position);
					OnAppearing?.Invoke(position);
				}
			}
			else
			{
				if (_visibleAdapterPosition.Contains(position))
				{
					_visibleAdapterPosition.Remove(position);
					OnDisappearing?.Invoke(position);
				}
			}
		}
		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		internal event Action<int> OnAppearing;
		internal event Action<int> OnBeginScroll;
		internal event Action<int> OnDisappearing;
		internal event Action<int> OnEndScroll;

		internal IntVector Velocity => _samples.Aggregate((o, a) => o + a) / _samples.Count;
		internal void Layout(int width, int height)
		{
			// e.g. when rotated the width and height are updated the virtual layout will 
			// need to resize and provide a new viewport offset given the current one.
			_virtualLayout.Layout(_positionOrigin, new IntSize(width, height), ref _locationOffset);
		}
		internal IntRectangle Viewport => Rectangle + _locationOffset;
		internal IEnumerable<int> VisiblePositions()
		{
			return _visibleAdapterPosition;
		}
		internal IEnumerable<AndroidView> Views()
		{
			return _viewByAdaptorPosition.Values;
		}

		public override void OnAdapterChanged(Adapter oldAdapter, Adapter newAdapter)
		{
			RemoveAllViews();
		}
		public override void OnItemsChanged(RecyclerView recyclerView)
		{
			_adapterChangeType = AdapterChangeType.Changed;

			// low-fidelity change event; assume everything has changed. If adapter reports it has "stable IDs" then 
			// RecyclerView will attempt to synthesize high-fidelity change events: added, removed, moved, updated.
			base.OnItemsChanged(recyclerView);
		}
		public override void OnItemsAdded(RecyclerView recyclerView, int positionStart, int itemCount)
		{
			_adapterChangeType = AdapterChangeType.Added;

			_deferredLayout.Enqueue((recycler, state) => {

				var viewByAdaptorPositionCopy = _viewByAdaptorPosition.ToArray();
				_viewByAdaptorPosition.Clear();
				foreach (KeyValuePair<int, AndroidView> pair in viewByAdaptorPositionCopy)
				{
					var view = pair.Value;
					var position = pair.Key;

					// position unchanged
					if (position < positionStart)
						_viewByAdaptorPosition[position] = view;

					// position changed
					else
						_viewByAdaptorPosition[position + itemCount] = view;
				}

				if (_positionOrigin >= positionStart)
					_positionOrigin += itemCount;
			});
			base.OnItemsAdded(recyclerView, positionStart, itemCount);
		}
		public override void OnItemsRemoved(RecyclerView recyclerView, int positionStart, int itemCount)
		{
			Assert(itemCount == MaxItemsRemoved);
			_adapterChangeType = AdapterChangeType.Removed;

			var positionEnd = positionStart + itemCount;

			_deferredLayout.Enqueue((recycler, state) => {
				if (state.ItemCount == 0)
					throw new InvalidOperationException("Cannot delete all items.");

				// re-map views to their new positions
				var viewByAdaptorPositionCopy = _viewByAdaptorPosition.ToArray();
				_viewByAdaptorPosition.Clear();
				foreach (var pair in viewByAdaptorPositionCopy)
				{
					var view = pair.Value;
					var position = pair.Key;

					// position unchanged
					if (position < positionStart)
						_viewByAdaptorPosition[position] = view;

					// position changed
					else if (position >= positionEnd)
						_viewByAdaptorPosition[position - itemCount] = view;

					// removed
					else
					{
						_viewByAdaptorPosition[-1] = view;
						if (_visibleAdapterPosition.Contains(position))
							_visibleAdapterPosition.Remove(position);
					}
				}

				// if removed origin then shift origin to first removed position
				if (_positionOrigin >= positionStart && _positionOrigin < positionEnd)
				{
					_positionOrigin = positionStart;

					// if no items to right of removed origin then set origin to item prior to removed set
					if (_positionOrigin >= state.ItemCount)
					{
						_positionOrigin = state.ItemCount - 1;

						if (!_viewByAdaptorPosition.ContainsKey(_positionOrigin))
							throw new InvalidOperationException(
								"VirtualLayoutManager must add items to the left and right of the origin"
							);
					}
				}

				// if removed before origin then shift origin left
				else if (_positionOrigin >= positionEnd)
					_positionOrigin -= itemCount;
			});

			base.OnItemsRemoved(recyclerView, positionStart, itemCount);
		}
		public override void OnItemsMoved(RecyclerView recyclerView, int from, int toValue, int itemCount)
		{
			_adapterChangeType = AdapterChangeType.Moved;
			base.OnItemsMoved(recyclerView, from, toValue, itemCount);
		}
		public override void OnItemsUpdated(RecyclerView recyclerView, int positionStart, int itemCount)
		{
			_adapterChangeType = AdapterChangeType.Updated;

			// rebind rendered updated elements
			_deferredLayout.Enqueue((recycler, state) => {
				for (var i = 0; i < itemCount; i++)
				{
					var position = positionStart + i;

					AndroidView view;
					if (!_viewByAdaptorPosition.TryGetValue(position, out view))
						continue;

					recycler.BindViewToPosition(view, position);
				}
			});

			base.OnItemsUpdated(recyclerView, positionStart, itemCount);
		}

		public override LayoutParams GenerateDefaultLayoutParams()
		{
			return new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
		}
		public override AndroidView FindViewByPosition(int adapterPosition)
		{
			// Used by SmoothScrollToPosition to know when the view 
			// for the targeted adapterPosition has been attached.

			AndroidView view;
			if (!_viewByAdaptorPosition.TryGetValue(adapterPosition, out view))
				return null;
			return view;
		}

		public override void ScrollToPosition(int adapterPosition)
		{
			if (adapterPosition < 0 || adapterPosition >= ItemCount)
				throw new ArgumentException(nameof(adapterPosition));

			_scroller.TargetPosition = adapterPosition;
			StartSmoothScroll(_scroller);
		}
		public override void SmoothScrollToPosition(RecyclerView recyclerView, State state, int adapterPosition)
		{
			ScrollToPosition(adapterPosition);
		}
		public override bool CanScrollHorizontally() => _virtualLayout.CanScrollHorizontally;
		public override bool CanScrollVertically() => _virtualLayout.CanScrollVertically;

		// entry points
		public override bool SupportsPredictiveItemAnimations() => true;
		public override void OnLayoutChildren(Recycler recycler, State state)
		{
			var adapterChangeType = _adapterChangeType;
			if (state.IsPreLayout)
				adapterChangeType = default(AdapterChangeType);

			// adapter updates
			if (!state.IsPreLayout)
			{
				while (_deferredLayout.Count > 0)
					_deferredLayout.Dequeue()(recycler, state);
			}

			// get visible items
			var positions = _virtualLayout.GetPositions(
				positionOrigin: _positionOrigin,
				itemCount: state.ItemCount,
				viewport: Viewport,
				// IsPreLayout => some type of data update of yet unknown type. Must assume update 
				// could be remove so virtualLayout must +1 off-screen left in case origin is 
				// removed and +n off-screen right to slide onscreen if a big item is removed
				isPreLayout: state.IsPreLayout || adapterChangeType == AdapterChangeType.Removed
			).ToRange();

			// disappearing
			var disappearing = _viewByAdaptorPosition.Keys.Except(positions).ToList();

			// defer cleanup of displaced items and lay them out off-screen so they animate off-screen
			if (adapterChangeType == AdapterChangeType.Added)
			{
				positions = positions.Concat(disappearing).OrderBy(o => o).ToArray();
				disappearing.Clear();
			}

			// recycle
			foreach (var position in disappearing)
			{
				var view = _viewByAdaptorPosition[position];

				// remove
				_viewByAdaptorPosition.Remove(position);
				OnAppearingOrDisappearing(position, false);

				// scrap
				new DecoratedView(this, view).DetachAndScrap(recycler);
			}

			// TODO: Generalize
			if (adapterChangeType == AdapterChangeType.Removed && _positionOrigin == state.ItemCount - 1)
			{
				var vlayout = _virtualLayout.LayoutItem(_positionOrigin, _positionOrigin);
				_locationOffset = new IntVector(vlayout.Width - Width, _locationOffset.Y);
			}

			var nextLocationOffset = new IntPoint(int.MaxValue, int.MaxValue);
			var nextPositionOrigin = int.MaxValue;
			foreach (var position in positions)
			{
				// attach
				AndroidView view;
				if (!_viewByAdaptorPosition.TryGetValue(position, out view))
					AddView(_viewByAdaptorPosition[position] = view = recycler.GetViewForPosition(position));

				// layout
				var decoratedView = new DecoratedView(this, view);
				var layout = _virtualLayout.LayoutItem(_positionOrigin, position);
				var physicalLayout = layout - _locationOffset;
				decoratedView.Layout(physicalLayout);

				var isVisible = Viewport.IntersectsWith(layout);
				if (isVisible)
					OnAppearingOrDisappearing(position, true);

				// update offsets
				if (isVisible && position < nextPositionOrigin)
				{
					nextLocationOffset = layout.Location;
					nextPositionOrigin = position;
				}
			}

			// update origin
			if (nextPositionOrigin != int.MaxValue)
			{
				_positionOrigin = nextPositionOrigin;
				_locationOffset -= (IntVector)nextLocationOffset;
			}

			// scrapped views not re-attached must be recycled (why isn't this done by Android, I dunno)
			foreach (var viewHolder in recycler.ScrapList.ToArray())
				recycler.RecycleView(viewHolder.ItemView);
		}

		public override int ScrollHorizontallyBy(int dx, Recycler recycler, State state)
		{
			var delta = new IntVector(dx, 0);
			ScrollBy(ref delta, recycler, state);
			return delta.X;
		}
		public override int ScrollVerticallyBy(int dy, Recycler recycler, State state)
		{
			var delta = new IntVector(0, dy);
			ScrollBy(ref delta, recycler, state);
			return delta.Y;
		}

		public override string ToString()
		{
			return $"offset={_locationOffset}";
		}
	}

	internal class ItemViewAdapter : Adapter
	{
		#region Private Definitions
		class CarouselViewHolder : ViewHolder
		{
			#region Fields
			readonly View _view;
			readonly IVisualElementRenderer _visualElementRenderer;
			#endregion

			public CarouselViewHolder(View view, IVisualElementRenderer renderer) 
				: base(renderer.ViewGroup)
			{
				_visualElementRenderer = renderer;
				_view = view;
			}

			public View View => _view;
			public IVisualElementRenderer VisualElementRenderer => _visualElementRenderer;
		}
		#endregion

		#region Fields
		readonly IVisualElementRenderer _renderer;
		readonly Dictionary<int, object> _typeByTypeId;
		readonly Dictionary<object, int> _typeIdByType;
		int _nextItemTypeId;
		#endregion

		public ItemViewAdapter(IVisualElementRenderer carouselRenderer)
		{
			_renderer = carouselRenderer;
			_typeByTypeId = new Dictionary<int, object>();
			_typeIdByType = new Dictionary<object, int>();
			_nextItemTypeId = 0;
		}

		#region Private Members
		ItemsView Element
		{
			get {
				return (ItemsView)_renderer.Element;
			}
		}
		IItemViewController Controller
		{
			get {
				return Element;
			}
		}
		#endregion

		public override int ItemCount
		{
			get {
				return Element.Count;
			}
		}
		public override int GetItemViewType(int position)
		{
			// get item and type from ItemSource and ItemTemplate
			object item = Controller.GetItem(position);
			object type = Controller.GetItemType(item);

			// map type as DataTemplate to type as Id
			int id = default(int);
			if (!_typeIdByType.TryGetValue(type, out id))
			{
				id = _nextItemTypeId++;
				_typeByTypeId[id] = type;
				_typeIdByType[type] = id;
			}
			return id;
		}
		public override ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			// create view from type
			var type = _typeByTypeId[viewType];
			var view = Controller.CreateView(type);

			// create renderer for view
			var renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, renderer);

			// package renderer + view
			return new CarouselViewHolder(view, renderer);
		}
		public override void OnBindViewHolder(ViewHolder holder, int position)
		{
			var carouselHolder = (CarouselViewHolder)holder;

			var item = Controller.GetItem(position);
			Controller.BindView(carouselHolder.View, item);
		}
	}
}