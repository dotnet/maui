using System;
using Android.Support.V7.Widget;
using Object = Java.Lang.Object;

namespace Xamarin.Forms.Platform.Android
{
	internal class DataChangeObserver : RecyclerView.AdapterDataObserver
	{
		readonly Action _onDataChange;

		public DataChangeObserver(Action onDataChange) : base()
		{
			_onDataChange = onDataChange;
		}

		public override void OnChanged()
		{
			base.OnChanged();
			_onDataChange?.Invoke();
		}

		public override void OnItemRangeInserted(int positionStart, int itemCount)
		{
			base.OnItemRangeInserted(positionStart, itemCount);
			_onDataChange?.Invoke();
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount)
		{
			base.OnItemRangeChanged(positionStart, itemCount);
			_onDataChange?.Invoke();
		}

		public override void OnItemRangeChanged(int positionStart, int itemCount, Object payload)
		{
			base.OnItemRangeChanged(positionStart, itemCount, payload);
			_onDataChange?.Invoke();
		}

		public override void OnItemRangeRemoved(int positionStart, int itemCount)
		{
			base.OnItemRangeRemoved(positionStart, itemCount);
			_onDataChange?.Invoke();
		}

		public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount)
		{
			base.OnItemRangeMoved(fromPosition, toPosition, itemCount);
			_onDataChange?.Invoke();
		}
	}
}