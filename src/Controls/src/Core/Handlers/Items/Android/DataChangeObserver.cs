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
		Adapter _observedAdapter;
		readonly Action _onDataChange;
		readonly Func<int> _trailingWindow;

		public bool Observing { get; private set; }

		// trailingWindow: when set, only report items inserted or removed within that many positions of the end of the adapter
		public DataChangeObserver(Action onDataChange, Func<int> trailingWindow = null) : base()
		{
			_onDataChange = onDataChange;
			_trailingWindow = trailingWindow;
		}

		public void Start(Adapter adapter)
		{
			if (Observing)
			{
				return;
			}

			adapter.RegisterAdapterDataObserver(this);

			_adapter = adapter.Handle;
			_observedAdapter = adapter;
			Observing = true;
		}

		public void Stop(Adapter adapter)
		{
			if (Observing && IsValidAdapter(adapter))
			{
				adapter.UnregisterAdapterDataObserver(this);
			}

			_adapter = IntPtr.Zero;
			_observedAdapter = null;
			Observing = false;
		}

		public override void OnChanged()
		{
			base.OnChanged();

			if (_trailingWindow is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeInserted(int positionStart, int itemCount)
		{
			base.OnItemRangeInserted(positionStart, itemCount);

			if (_trailingWindow is null || positionStart + _trailingWindow() > ItemCount - itemCount)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount)
		{
			base.OnItemRangeChanged(positionStart, itemCount);

			if (_trailingWindow is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount, Object payload)
		{
			base.OnItemRangeChanged(positionStart, itemCount, payload);

			if (_trailingWindow is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeRemoved(int positionStart, int itemCount)
		{
			base.OnItemRangeRemoved(positionStart, itemCount);

			if (_trailingWindow is null || positionStart + _trailingWindow() > ItemCount)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
		{
			base.OnItemRangeMoved(fromPosition, toPosition, itemCount);

			if (_trailingWindow is null)
			{
				_onDataChange?.Invoke();
			}
		}

		int ItemCount => _observedAdapter?.ItemCount ?? 0;

		bool IsValidAdapter(Adapter adapter)
		{
			return adapter != null && adapter.Handle == _adapter && adapter.HasObservers;
		}
	}
}