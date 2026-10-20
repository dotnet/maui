#nullable disable
using System;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
    // Owns the cached classification of "which adapter positions are supplementary
    // (Header/Footer/GroupHeader/GroupFooter/EmptyView)" so the accessibility delegate
    // answers TalkBack queries in O(1) instead of O(N). See dotnet/maui#35681.
    internal sealed class CollectionViewAccessibilityMetadata
    {
        readonly RecyclerView _recyclerView;
        readonly DataObserver _observer;

        RecyclerView.Adapter _observedAdapter;

        // _nonDataBefore[i] = count of supplementary items in adapter positions [0, i).
        // Length is (adapter.ItemCount + 1) so IsSupplementary(pos) can peek at pos+1.
        int[] _nonDataBefore;
        bool _hasCache;

        public CollectionViewAccessibilityMetadata(RecyclerView recyclerView)
        {
            _recyclerView = recyclerView;
            _observer = new DataObserver(this);
        }

        // Called by MauiRecyclerView whenever the adapter is swapped
        // (data <-> EmptyViewAdapter, teardown, etc.).
        public void OnAdapterAttached(RecyclerView.Adapter newAdapter)
        {
            if (ReferenceEquals(_observedAdapter, newAdapter))
            {
                return;
            }

            if (_observedAdapter is not null)
            {
                try
                {
                    _observedAdapter.UnregisterAdapterDataObserver(_observer);
                }
                catch
                {
                    // The old adapter may already be disposed or not registered.
                }
            }

            _observedAdapter = newAdapter;
            if (newAdapter is not null)
            {
                newAdapter.RegisterAdapterDataObserver(_observer);
            }

            InvalidateCache();
        }

        public bool HasSupplementaryItems
        {
            get
            {
                EnsureCache();
                return _nonDataBefore is not null && _nonDataBefore.Length > 0 && _nonDataBefore[_nonDataBefore.Length - 1] > 0;
            }
        }

        public int SupplementaryItemCount
        {
            get
            {
                EnsureCache();
                return _nonDataBefore is null || _nonDataBefore.Length == 0 ? 0 : _nonDataBefore[_nonDataBefore.Length - 1];
            }
        }

        public int GetNonDataItemsBefore(int position)
        {
            EnsureCache();
            if (_nonDataBefore is null || position <= 0)
            {
                return 0;
            }

            if (position >= _nonDataBefore.Length)
            {
                return _nonDataBefore[_nonDataBefore.Length - 1];
            }

            return _nonDataBefore[position];
        }

        public bool IsSupplementary(int position)
        {
            EnsureCache();
            if (_nonDataBefore is null || position < 0 || position + 1 >= _nonDataBefore.Length)
            {
                return false;
            }

            return _nonDataBefore[position + 1] > _nonDataBefore[position];
        }

        void InvalidateCache()
        {
            _hasCache = false;
            _nonDataBefore = null;
        }

        void EnsureCache()
        {
            if (_hasCache)
            {
                return;
            }

            var adapter = _recyclerView.GetAdapter();
            if (adapter is null || adapter.ItemCount == 0)
            {
                _nonDataBefore = Array.Empty<int>();
                _hasCache = true;
                return;
            }

            var itemCount = adapter.ItemCount;
            var prefix = new int[itemCount + 1];
            int running = 0;
            var treatAllAsSupplementary = adapter is EmptyViewAdapter;
            for (int i = 0; i < itemCount; i++)
            {
                prefix[i] = running;
                if (treatAllAsSupplementary || IsNonDataViewType(adapter.GetItemViewType(i)))
                {
                    running++;
                }
            }

            prefix[itemCount] = running;
            _nonDataBefore = prefix;
            _hasCache = true;
        }

        static bool IsNonDataViewType(int viewType)
        {
            return viewType == ItemViewType.Header
                || viewType == ItemViewType.Footer
                || viewType == ItemViewType.GroupHeader
                || viewType == ItemViewType.GroupFooter;
        }

        sealed class DataObserver : RecyclerView.AdapterDataObserver
        {
            readonly CollectionViewAccessibilityMetadata _parent;
            public DataObserver(CollectionViewAccessibilityMetadata parent)
            {
                _parent = parent;
            }

            public override void OnChanged() => _parent.InvalidateCache();
            public override void OnItemRangeChanged(int positionStart, int itemCount) => _parent.InvalidateCache();
            public override void OnItemRangeChanged(int positionStart, int itemCount, Java.Lang.Object payload) => _parent.InvalidateCache();
            public override void OnItemRangeInserted(int positionStart, int itemCount) => _parent.InvalidateCache();
            public override void OnItemRangeRemoved(int positionStart, int itemCount) => _parent.InvalidateCache();
            public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount) => _parent.InvalidateCache();
        }
    }
}
