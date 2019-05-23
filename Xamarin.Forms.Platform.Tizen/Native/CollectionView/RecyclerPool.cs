using System.Collections.Generic;
using System.Linq;

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

		public ViewHolder GetRecyclerView(object category)
		{
			var holder = _pool.Where(d => d.ViewCategory == category).FirstOrDefault();
			if (holder != null)
				_pool.Remove(holder);
			return holder;
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
