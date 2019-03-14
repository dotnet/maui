using System.Collections.Generic;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	class RecyclerPool
	{
		LinkedList<ViewHolder> _pool = new LinkedList<ViewHolder>();

		public void Clear(ItemAdaptor adaptor)
		{
			foreach (var item in _pool)
			{
				adaptor.RemoveNativeView(item);
			}
			_pool.Clear();
		}

		public void AddRecyclerView(ViewHolder view)
		{
			_pool.AddLast(view);
		}

		public ViewHolder GetRecyclerView()
		{
			if (_pool.First != null)
			{
				var fisrt = _pool.First;
				_pool.RemoveFirst();
				return fisrt.Value;
			}
			return null;
		}
	}
}
