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
		readonly Func<int> _lastRowStart;

		public bool Observing { get; private set; }

		// lastRowStart: when set, only report items inserted or removed after that adapter position (read after the change)
		public DataChangeObserver(Action onDataChange, Func<int> lastRowStart = null) : base()
		{
			_onDataChange = onDataChange;
			_lastRowStart = lastRowStart;
		}

		public void Start(Adapter adapter)
		{
			if (Observing)
			{
				return;
			}

			adapter.RegisterAdapterDataObserver(this);

			_adapter = adapter.Handle;
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

			if (_lastRowStart is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeInserted(int positionStart, int itemCount)
		{
			base.OnItemRangeInserted(positionStart, itemCount);

			if (_lastRowStart is null || positionStart + itemCount > _lastRowStart())
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount)
		{
			base.OnItemRangeChanged(positionStart, itemCount);

			if (_lastRowStart is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount, Object payload)
		{
			base.OnItemRangeChanged(positionStart, itemCount, payload);

			if (_lastRowStart is null)
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeRemoved(int positionStart, int itemCount)
		{
			base.OnItemRangeRemoved(positionStart, itemCount);

			if (_lastRowStart is null || positionStart > _lastRowStart())
			{
				_onDataChange?.Invoke();
			}
		}

		public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
		{
			base.OnItemRangeMoved(fromPosition, toPosition, itemCount);

			if (_lastRowStart is null)
			{
				_onDataChange?.Invoke();
			}
		}

		bool IsValidAdapter(Adapter adapter)
		{
			return adapter != null && adapter.Handle == _adapter && adapter.HasObservers;
		}
	}
}