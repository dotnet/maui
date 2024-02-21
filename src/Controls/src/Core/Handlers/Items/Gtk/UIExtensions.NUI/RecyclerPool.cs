using System.Collections.Generic;
using System.Linq;

namespace Gtk.UIExtensions.NUI
{
    class RecyclerPool
    {
        LinkedList<ViewHolder> _pool = new LinkedList<ViewHolder>();

        public int Count { get; private set; }

        public void Clear(ItemAdaptor adaptor)
        {
            foreach (var item in _pool)
            {
                adaptor.RemoveNativeView(item.Content!);
                item.Unparent();
                item.Dispose();
            }
            _pool.Clear();
        }

        public void AddRecyclerView(ViewHolder view)
        {
            Count++;
            _pool.AddLast(view);
        }

        public ViewHolder? GetRecyclerView(object category)
        {
            var holder = _pool.Where(d => d.ViewCategory == category).FirstOrDefault();
            if (holder != null)
            {
                _pool.Remove(holder);
                Count--;
            }
            return holder;
        }

        public ViewHolder? GetRecyclerView()
        {
            if (_pool.First != null)
            {
                var fisrt = _pool.First;
                _pool.RemoveFirst();
                Count--;
                return fisrt.Value;
            }
            return null;
        }
    }
}
