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
		readonly Action _onDataChange;
		readonly Func<int> _lastRowSize;
		readonly Func<int> _getItemCount;
		int _itemCount = -1;

		public bool Observing { get; private set; }

		// lastRowSize: when set, only report items inserted into or removed from the last row (that many positions, 0 for none).
		// The item count is tracked from the notifications; getItemCount is only called when it is unknown.
		public DataChangeObserver(Action onDataChange, Func<int> lastRowSize = null, Func<int> getItemCount = null) : base()
		{
			_onDataChange = onDataChange;
			_lastRowSize = lastRowSize;
			_getItemCount = getItemCount;
		}

		public void Start(Adapter adapter)
		{
			if (Observing)
			{
				return;
			}

			adapter.RegisterAdapterDataObserver(this);

			_adapter = adapter.Handle;
			_itemCount = -1;
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
			_itemCount = -1;

			if (_lastRowSize is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeInserted(int positionStart, int itemCount)
		{
			base.OnItemRangeInserted(positionStart, itemCount);

			if (_itemCount >= 0)
			{
				_itemCount += itemCount;
			}

			if (_lastRowSize is null || ReachesLastRow(positionStart + itemCount))
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount)
		{
			base.OnItemRangeChanged(positionStart, itemCount);

			if (_lastRowSize is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount, Object payload)
		{
			base.OnItemRangeChanged(positionStart, itemCount, payload);

			if (_lastRowSize is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeRemoved(int positionStart, int itemCount)
		{
			base.OnItemRangeRemoved(positionStart, itemCount);

			if (_itemCount >= 0)
			{
				_itemCount -= itemCount;
			}

			if (_lastRowSize is null || ReachesLastRow(positionStart))
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
		{
			base.OnItemRangeMoved(fromPosition, toPosition, itemCount);

			if (_lastRowSize is null)
			{
				_onDataChange?.Invoke();
			}
		}

		// True when the range ending at `position` (exclusive) reaches the last row
		bool ReachesLastRow(int position)
		{
			int lastRowSize = _lastRowSize();
			if (lastRowSize <= 0)
			{
				return false;
			}

			if (_itemCount < 0)
			{
				_itemCount = _getItemCount?.Invoke() ?? 0;
			}

			return position > _itemCount - lastRowSize;
		}

		bool IsValidAdapter(Adapter adapter)
		{
			return adapter != null && adapter.Handle == _adapter && adapter.HasObservers;
		}
	}
}