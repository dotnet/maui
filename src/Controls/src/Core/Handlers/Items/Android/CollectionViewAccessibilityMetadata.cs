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

        // _nonDataBefore[i] = count of supplementary items in adapter positions [0, i).
        // Length is (adapter.ItemCount + 1) so IsSupplementary(pos) can peek at pos+1.
        int[] _nonDataBefore;
        bool _hasCache;

        internal CollectionViewAccessibilityMetadata(RecyclerView recyclerView)
        {
            _recyclerView = recyclerView;
            _observer = new DataObserver(this);
        }

        // Observe each adapter for its lifetime. EmptyView swaps can happen inside a
        // notification, when removing another observer would corrupt RecyclerView's iteration.
        internal void ObserveAdapter(RecyclerView.Adapter adapter)
        {
            adapter?.RegisterAdapterDataObserver(_observer);
            InvalidateCache();
        }

        internal void UnobserveAdapter(RecyclerView.Adapter adapter)
        {
            adapter?.UnregisterAdapterDataObserver(_observer);
            InvalidateCache();
        }

        internal bool HasSupplementaryItems
        {
            get
            {
                EnsureCache();
                return _nonDataBefore is not null && _nonDataBefore.Length > 0 && _nonDataBefore[_nonDataBefore.Length - 1] > 0;
            }
        }

        internal int SupplementaryItemCount
        {
            get
            {
                EnsureCache();
                return _nonDataBefore is null || _nonDataBefore.Length == 0 ? 0 : _nonDataBefore[_nonDataBefore.Length - 1];
            }
        }

        internal int GetNonDataItemsBefore(int position)
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

        internal bool IsSupplementary(int position)
        {
            EnsureCache();
            if (_nonDataBefore is null || position < 0 || position + 1 >= _nonDataBefore.Length)
            {
                return false;
            }

            return _nonDataBefore[position + 1] > _nonDataBefore[position];
        }

        internal void InvalidateCache()
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

        bool IsNonDataViewType(int viewType)
        {
            return viewType == ItemViewType.Header
                || viewType == ItemViewType.Footer
                || viewType == ItemViewType.GroupHeader
                || viewType == ItemViewType.GroupFooter;
        }

        sealed class DataObserver : RecyclerView.AdapterDataObserver
        {
            readonly CollectionViewAccessibilityMetadata _parent;
            internal DataObserver(CollectionViewAccessibilityMetadata parent)
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
