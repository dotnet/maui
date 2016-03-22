using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal class PhysicalLayoutManager : RecyclerView.LayoutManager
	{
		// ObservableCollection is our public entryway to this method and it only supports single item removal
		internal const int MaxItemsRemoved = 1;

		static readonly int s_samplesCount = 5;
		static Func<int, int> s_fixPosition = o => o;

		readonly Context _context;
		readonly Queue<Action<RecyclerView.Recycler, RecyclerView.State>> _deferredLayout;
		readonly List<IntVector> _samples;
		readonly SeekAndSnapScroller _scroller;
		readonly Dictionary<int, global::Android.Views.View> _viewByAdaptorPosition;
		readonly VirtualLayoutManager _virtualLayout;
		readonly HashSet<int> _visibleAdapterPosition;
		AdapterChangeType _adapterChangeType;
		IntVector _locationOffset; // upper left corner of screen is positionOrigin + locationOffset
		int _positionOrigin; // coordinates are relative to the upper left corner of this element

		public PhysicalLayoutManager(Context context, VirtualLayoutManager virtualLayout, int positionOrigin)
		{
			_positionOrigin = positionOrigin;
			_context = context;
			_virtualLayout = virtualLayout;
			_viewByAdaptorPosition = new Dictionary<int, global::Android.Views.View>();
			_visibleAdapterPosition = new HashSet<int>();
			_samples = Enumerable.Repeat(IntVector.Origin, s_samplesCount).ToList();
			_deferredLayout = new Queue<Action<RecyclerView.Recycler, RecyclerView.State>>();
			_scroller = new SeekAndSnapScroller(context, adapterPosition =>
			{
				IntVector end = virtualLayout.LayoutItem(positionOrigin, adapterPosition).Center();
				IntVector begin = Viewport.Center();
				return end - begin;
			});

			_scroller.OnBeginScroll += adapterPosition => OnBeginScroll?.Invoke(adapterPosition);
			_scroller.OnEndScroll += adapterPosition => OnEndScroll?.Invoke(adapterPosition);
		}

		public IntVector Velocity => _samples.Aggregate((o, a) => o + a) / _samples.Count;

		public System.Drawing.Rectangle Viewport => Rectangle + _locationOffset;

		// helpers to deal with locations as IntRectangles and IntVectors
		System.Drawing.Rectangle Rectangle => new System.Drawing.Rectangle(0, 0, Width, Height);

		public override bool CanScrollHorizontally() => _virtualLayout.CanScrollHorizontally;

		public override bool CanScrollVertically() => _virtualLayout.CanScrollVertically;

		public override global::Android.Views.View FindViewByPosition(int adapterPosition)
		{
			// Used by SmoothScrollToPosition to know when the view 
			// for the targeted adapterPosition has been attached.

			global::Android.Views.View view;
			if (!_viewByAdaptorPosition.TryGetValue(adapterPosition, out view))
				return null;
			return view;
		}

		public override RecyclerView.LayoutParams GenerateDefaultLayoutParams()
		{
			return new RecyclerView.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
		}

		public void Layout(int width, int height)
		{
			// e.g. when rotated the width and height are updated the virtual layout will 
			// need to resize and provide a new viewport offset given the current one.
			_virtualLayout.Layout(_positionOrigin, new System.Drawing.Size(width, height), ref _locationOffset);
		}

		public override void OnAdapterChanged(RecyclerView.Adapter oldAdapter, RecyclerView.Adapter newAdapter)
		{
			RemoveAllViews();
		}

		public event Action<int> OnAppearing;

		public event Action<int> OnBeginScroll;

		public event Action<int> OnDisappearing;

		public event Action<int> OnEndScroll;

		public override void OnItemsAdded(RecyclerView recyclerView, int positionStart, int itemCount)
		{
			_adapterChangeType = AdapterChangeType.Added;

			_deferredLayout.Enqueue((recycler, state) =>
			{
				KeyValuePair<int, global::Android.Views.View>[] viewByAdaptorPositionCopy = _viewByAdaptorPosition.ToArray();
				_viewByAdaptorPosition.Clear();
				foreach (KeyValuePair<int, global::Android.Views.View> pair in viewByAdaptorPositionCopy)
				{
					global::Android.Views.View view = pair.Value;
					int position = pair.Key;

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

		public override void OnItemsChanged(RecyclerView recyclerView)
		{
			_adapterChangeType = AdapterChangeType.Changed;

			// low-fidelity change event; assume everything has changed. If adapter reports it has "stable IDs" then 
			// RecyclerView will attempt to synthesize high-fidelity change events: added, removed, moved, updated.
			base.OnItemsChanged(recyclerView);
		}

		public override void OnItemsMoved(RecyclerView recyclerView, int from, int toValue, int itemCount)
		{
			_adapterChangeType = AdapterChangeType.Moved;
			base.OnItemsMoved(recyclerView, from, toValue, itemCount);
		}

		public override void OnItemsRemoved(RecyclerView recyclerView, int positionStart, int itemCount)
		{
			Debug.Assert(itemCount == MaxItemsRemoved);
			_adapterChangeType = AdapterChangeType.Removed;

			int positionEnd = positionStart + itemCount;

			_deferredLayout.Enqueue((recycler, state) =>
			{
				if (state.ItemCount == 0)
					throw new InvalidOperationException("Cannot delete all items.");

				// re-map views to their new positions
				KeyValuePair<int, global::Android.Views.View>[] viewByAdaptorPositionCopy = _viewByAdaptorPosition.ToArray();
				_viewByAdaptorPosition.Clear();
				foreach (KeyValuePair<int, global::Android.Views.View> pair in viewByAdaptorPositionCopy)
				{
					global::Android.Views.View view = pair.Value;
					int position = pair.Key;

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
							throw new InvalidOperationException("VirtualLayoutManager must add items to the left and right of the origin");
					}
				}

				// if removed before origin then shift origin left
				else if (_positionOrigin >= positionEnd)
					_positionOrigin -= itemCount;
			});

			base.OnItemsRemoved(recyclerView, positionStart, itemCount);
		}

		public override void OnItemsUpdated(RecyclerView recyclerView, int positionStart, int itemCount)
		{
			_adapterChangeType = AdapterChangeType.Updated;

			// rebind rendered updated elements
			_deferredLayout.Enqueue((recycler, state) =>
			{
				for (var i = 0; i < itemCount; i++)
				{
					int position = positionStart + i;

					global::Android.Views.View view;
					if (!_viewByAdaptorPosition.TryGetValue(position, out view))
						continue;

					recycler.BindViewToPosition(view, position);
				}
			});

			base.OnItemsUpdated(recyclerView, positionStart, itemCount);
		}

		public override void OnLayoutChildren(RecyclerView.Recycler recycler, RecyclerView.State state)
		{
			AdapterChangeType adapterChangeType = _adapterChangeType;
			if (state.IsPreLayout)
				adapterChangeType = default(AdapterChangeType);

			// adapter updates
			if (!state.IsPreLayout)
			{
				while (_deferredLayout.Count > 0)
					_deferredLayout.Dequeue()(recycler, state);
			}

			// get visible items
			int[] positions = _virtualLayout.GetPositions(_positionOrigin, state.ItemCount, Viewport,
				// IsPreLayout => some type of data update of yet unknown type. Must assume update 
				// could be remove so virtualLayout must +1 off-screen left in case origin is 
				// removed and +n off-screen right to slide onscreen if a big item is removed
				state.IsPreLayout || adapterChangeType == AdapterChangeType.Removed).ToRange();

			// disappearing
			List<int> disappearing = _viewByAdaptorPosition.Keys.Except(positions).ToList();

			// defer cleanup of displaced items and lay them out off-screen so they animate off-screen
			if (adapterChangeType == AdapterChangeType.Added)
			{
				positions = positions.Concat(disappearing).OrderBy(o => o).ToArray();
				disappearing.Clear();
			}

			// recycle
			foreach (int position in disappearing)
			{
				global::Android.Views.View view = _viewByAdaptorPosition[position];

				// remove
				_viewByAdaptorPosition.Remove(position);
				OnAppearingOrDisappearing(position, false);

				// scrap
				new DecoratedView(this, view).DetachAndScrap(recycler);
			}

			// TODO: Generalize
			if (adapterChangeType == AdapterChangeType.Removed && _positionOrigin == state.ItemCount - 1)
			{
				System.Drawing.Rectangle vlayout = _virtualLayout.LayoutItem(_positionOrigin, _positionOrigin);
				_locationOffset = new IntVector(vlayout.Width - Width, _locationOffset.Y);
			}

			var nextLocationOffset = new System.Drawing.Point(int.MaxValue, int.MaxValue);
			int nextPositionOrigin = int.MaxValue;
			foreach (int position in positions)
			{
				// attach
				global::Android.Views.View view;
				if (!_viewByAdaptorPosition.TryGetValue(position, out view))
					AddView(_viewByAdaptorPosition[position] = view = recycler.GetViewForPosition(position));

				// layout
				var decoratedView = new DecoratedView(this, view);
				System.Drawing.Rectangle layout = _virtualLayout.LayoutItem(_positionOrigin, position);
				System.Drawing.Rectangle physicalLayout = layout - _locationOffset;
				decoratedView.Layout(physicalLayout);

				bool isVisible = Viewport.IntersectsWith(layout);
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
			foreach (RecyclerView.ViewHolder viewHolder in recycler.ScrapList.ToArray())
				recycler.RecycleView(viewHolder.ItemView);
		}

		public override int ScrollHorizontallyBy(int dx, RecyclerView.Recycler recycler, RecyclerView.State state)
		{
			var delta = new IntVector(dx, 0);
			ScrollBy(ref delta, recycler, state);
			return delta.X;
		}

		public override void ScrollToPosition(int adapterPosition)
		{
			if (adapterPosition < 0 || adapterPosition >= ItemCount)
				throw new ArgumentException(nameof(adapterPosition));

			_scroller.TargetPosition = adapterPosition;
			StartSmoothScroll(_scroller);
		}

		public override int ScrollVerticallyBy(int dy, RecyclerView.Recycler recycler, RecyclerView.State state)
		{
			var delta = new IntVector(0, dy);
			ScrollBy(ref delta, recycler, state);
			return delta.Y;
		}

		public override void SmoothScrollToPosition(RecyclerView recyclerView, RecyclerView.State state, int adapterPosition)
		{
			ScrollToPosition(adapterPosition);
		}

		// entry points
		public override bool SupportsPredictiveItemAnimations() => true;

		public override string ToString()
		{
			return $"offset={_locationOffset}";
		}

		public IEnumerable<global::Android.Views.View> Views()
		{
			return _viewByAdaptorPosition.Values;
		}

		public IEnumerable<int> VisiblePositions()
		{
			return _visibleAdapterPosition;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		void OffsetChildren(IntVector delta)
		{
			OffsetChildrenHorizontal(-delta.X);
			OffsetChildrenVertical(-delta.Y);
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

		void ScrollBy(ref IntVector delta, RecyclerView.Recycler recycler, RecyclerView.State state)
		{
			_adapterChangeType = default(AdapterChangeType);

			delta = Viewport.BoundTranslation(delta, _virtualLayout.GetBounds(_positionOrigin, state));

			_locationOffset += delta;
			_samples.Insert(0, delta);
			_samples.RemoveAt(_samples.Count - 1);

			OffsetChildren(delta);
			OnLayoutChildren(recycler, state);
		}

		enum AdapterChangeType
		{
			Removed = 1,
			Added,
			Moved,
			Updated,
			Changed
		}

		internal struct DecoratedView
		{
			public static implicit operator global::Android.Views.View(DecoratedView view)
			{
				return view._view;
			}

			readonly PhysicalLayoutManager _layout;
			readonly global::Android.Views.View _view;

			internal DecoratedView(PhysicalLayoutManager layout, global::Android.Views.View view)
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

			internal System.Drawing.Rectangle Rectangle => new System.Drawing.Rectangle(Left, Top, Width, Height);

			internal void Measure(int widthUsed, int heightUsed)
			{
				_layout.MeasureChild(_view, widthUsed, heightUsed);
			}

			internal void MeasureWithMargins(int widthUsed, int heightUsed)
			{
				_layout.MeasureChildWithMargins(_view, widthUsed, heightUsed);
			}

			internal void Layout(System.Drawing.Rectangle position)
			{
				var renderer = _view as IVisualElementRenderer;
				renderer.Element.Layout(position.ToFormsRectangle(_layout._context));

				_layout.LayoutDecorated(_view, position.Left, position.Top, position.Right, position.Bottom);
			}

			internal void Add()
			{
				_layout.AddView(_view);
			}

			internal void DetachAndScrap(RecyclerView.Recycler recycler)
			{
				_layout.DetachAndScrapView(_view, recycler);
			}
		}

		internal abstract class VirtualLayoutManager
		{
			internal abstract bool CanScrollHorizontally { get; }

			internal abstract bool CanScrollVertically { get; }

			internal abstract System.Drawing.Rectangle GetBounds(int positionOrigin, RecyclerView.State state);

			internal abstract Tuple<int, int> GetPositions(int positionOrigin, int itemCount, System.Drawing.Rectangle viewport, bool isPreLayout);

			internal abstract void Layout(int positionOrigin, System.Drawing.Size viewportSize, ref IntVector offset);

			internal abstract System.Drawing.Rectangle LayoutItem(int positionOrigin, int position);
		}

		enum SnapPreference
		{
			None = 0,
			Begin = 1,
			End = -1
		}

		sealed class SeekAndSnapScroller : LinearSmoothScroller
		{
			readonly SnapPreference _snapPreference;
			readonly Func<int, IntVector> _vectorToPosition;

			internal SeekAndSnapScroller(Context context, Func<int, IntVector> vectorToPosition, SnapPreference snapPreference = SnapPreference.None) : base(context)
			{
				_vectorToPosition = vectorToPosition;
				_snapPreference = snapPreference;
			}

			protected override int HorizontalSnapPreference => (int)_snapPreference;

			public override PointF ComputeScrollVectorForPosition(int targetPosition)
			{
				IntVector vector = _vectorToPosition(targetPosition);
				return new PointF(vector.X, vector.Y);
			}

			public event Action<int> OnBeginScroll;

			public event Action<int> OnEndScroll;

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
		}
	}
}