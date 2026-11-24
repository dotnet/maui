#nullable disable
using System;
using AndroidX.RecyclerView.Widget;
using static AndroidX.RecyclerView.Widget.RecyclerView;
using Object = Java.Lang.Object;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class DataChangeObserver : RecyclerView.AdapterDataObserver
	{
		IntPtr _adapter;
		readonly Action<bool> _onDataChange;
		readonly Func<int> _lastRowSize;

		public bool Observing { get; private set; }

		// Only used with lastRowSize: the adapter's item count as of the last layout pass, kept up to date with the insertions
		// and removals since then, or -1 when unknown
		public int ItemCount { get; set; } = -1;

		// lastRowSize: when set, only report items inserted into or removed from the last row (that many positions, 0 for none)
		public DataChangeObserver(Action<bool> onDataChange, Func<int> lastRowSize = null) : base()
		{
			_onDataChange = onDataChange;
			_lastRowSize = lastRowSize;
		}

		public void Start(Adapter adapter)
		{
			if (Observing)
			{
				return;
			}

			adapter.RegisterAdapterDataObserver(this);

			_adapter = adapter.Handle;
			ItemCount = -1;
			Observing = true;
		}

		public void Stop(Adapter adapter)
		{
			if (Observing && IsValidAdapter(adapter))
			{
				adapter.UnregisterAdapterDataObserver(this);
			}

			_adapter = IntPtr.Zero;
			Observing = false;
		}

		public override void OnChanged()
		{
			base.OnChanged();
			ItemCount = -1;

			if (_lastRowSize is null)
			{
				_onDataChange?.Invoke(false);
			}
		}

		public override void OnItemRangeInserted(int positionStart, int itemCount)
		{
			base.OnItemRangeInserted(positionStart, itemCount);

			if (ItemCount >= 0)
			{
				ItemCount += itemCount;
			}

			if (_lastRowSize is null || ReachesLastRow(positionStart + itemCount))
			{
				_onDataChange?.Invoke(true);
			}
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount)
		{
			base.OnItemRangeChanged(positionStart, itemCount);

			if (_lastRowSize is null)
			{
				_onDataChange?.Invoke(false);
			}
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount, Object payload)
		{
			base.OnItemRangeChanged(positionStart, itemCount, payload);

			if (_lastRowSize is null)
			{
				_onDataChange?.Invoke(false);
			}
		}

		public override void OnItemRangeRemoved(int positionStart, int itemCount)
		{
			base.OnItemRangeRemoved(positionStart, itemCount);

			if (ItemCount >= 0)
			{
				ItemCount -= itemCount;
			}

			if (_lastRowSize is null || ReachesLastRow(positionStart))
			{
				_onDataChange?.Invoke(false);
			}
		}

		public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
		{
			base.OnItemRangeMoved(fromPosition, toPosition, itemCount);

			if (_lastRowSize is null)
			{
				_onDataChange?.Invoke(false);
			}
		}

		// True when a range ending at `position` (exclusive) reaches the last row. Without a count from a layout pass yet,
		// report the change rather than risk stale spacing.
		bool ReachesLastRow(int position)
		{
			int lastRowSize = _lastRowSize();
			return lastRowSize > 0 && (ItemCount < 0 || position > ItemCount - lastRowSize);
		}

		bool IsValidAdapter(Adapter adapter)
		{
			return adapter != null && adapter.Handle == _adapter && adapter.HasObservers;
		}
	}
}