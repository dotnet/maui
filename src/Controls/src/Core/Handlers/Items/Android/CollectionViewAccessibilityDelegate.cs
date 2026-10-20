#nullable disable
using System;
using Android.Views.Accessibility;
using AndroidX.Core.View;
using AndroidX.Core.View.Accessibility;
using AndroidX.RecyclerView.Widget;
using AView = Android.Views.View;
using CollectionInfo = AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat.CollectionInfoCompat;
using CollectionItemInfo = AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat.CollectionItemInfoCompat;

namespace Microsoft.Maui.Controls.Handlers.Items
{
    // Translates cached metadata from CollectionViewAccessibilityMetadata into TalkBack-visible
    // CollectionInfo / CollectionItemInfo / AccessibilityEvent values.
    internal sealed class CollectionViewAccessibilityDelegate : RecyclerViewAccessibilityDelegate
    {
        readonly RecyclerView _recyclerView;
        readonly CollectionViewAccessibilityMetadata _accessibilityMetadata;
        readonly ItemAccessibilityDelegate _itemDelegate;

        public CollectionViewAccessibilityDelegate(RecyclerView recyclerView, CollectionViewAccessibilityMetadata metadata)
            : base(recyclerView)
        {
            _recyclerView = recyclerView;
            _accessibilityMetadata = metadata;
            _itemDelegate = new ItemAccessibilityDelegate(this);
        }

        public override AccessibilityDelegateCompat GetItemDelegate()
        {
            return _itemDelegate;
        }

        public override void OnInitializeAccessibilityNodeInfo(AView host, AccessibilityNodeInfoCompat info)
        {
            base.OnInitializeAccessibilityNodeInfo(host, info);

            if (info is null || !_accessibilityMetadata.HasSupplementaryItems)
            {
                return;
            }

            var oldInfo = info.CollectionInfo;
            if (oldInfo is null)
            {
                return;
            }

            var excludedCount = _accessibilityMetadata.SupplementaryItemCount;
            var isHorizontal = IsHorizontal(_recyclerView);

            int rowCount = isHorizontal
                ? oldInfo.RowCount
                : Math.Max(0, oldInfo.RowCount - excludedCount);

            int columnCount = isHorizontal
                ? Math.Max(0, oldInfo.ColumnCount - excludedCount)
                : oldInfo.ColumnCount;

            info.SetCollectionInfo(CollectionInfo.Obtain(
                rowCount,
                columnCount,
                oldInfo.IsHierarchical,
                oldInfo.SelectionMode));
        }

        public override void OnInitializeAccessibilityEvent(AView host, AccessibilityEvent e)
        {
            base.OnInitializeAccessibilityEvent(host, e);

            if (e is null || !_accessibilityMetadata.HasSupplementaryItems)
            {
                return;
            }

            e.ItemCount = Math.Max(0, e.ItemCount - _accessibilityMetadata.SupplementaryItemCount);
        }

        static bool IsHorizontal(RecyclerView recyclerView)
        {
            return recyclerView.GetLayoutManager() is LinearLayoutManager linearLayoutManager
                && linearLayoutManager.Orientation == LinearLayoutManager.Horizontal;
        }

        sealed class ItemAccessibilityDelegate : ItemDelegate
        {
            readonly CollectionViewAccessibilityDelegate _parent;

            public ItemAccessibilityDelegate(CollectionViewAccessibilityDelegate parent) : base(parent)
            {
                _parent = parent;
            }

            public override void OnInitializeAccessibilityNodeInfo(AView host, AccessibilityNodeInfoCompat info)
            {
                base.OnInitializeAccessibilityNodeInfo(host, info);

                var metadata = _parent._accessibilityMetadata;
                if (host is null || info is null || !metadata.HasSupplementaryItems)
                {
                    return;
                }

                var recyclerView = _parent._recyclerView;
                var position = recyclerView.GetChildAdapterPosition(host);
                if (position == RecyclerView.NoPosition)
                {
                    return;
                }

                if (metadata.IsSupplementary(position))
                {
                    info.SetCollectionItemInfo(null);
                    return;
                }

                var offset = metadata.GetNonDataItemsBefore(position);
                var oldItemInfo = info.CollectionItemInfo;

                if (offset == 0 || oldItemInfo is null)
                {
                    return;
                }

                var isHorizontal = IsHorizontal(recyclerView);

                int rowIndex = isHorizontal
                    ? oldItemInfo.RowIndex
                    : Math.Max(0, oldItemInfo.RowIndex - offset);

                int columnIndex = isHorizontal
                    ? Math.Max(0, oldItemInfo.ColumnIndex - offset)
                    : oldItemInfo.ColumnIndex;

#pragma warning disable CS0618 // Preserve the base heading metadata; IsHeading is the current API surface.
                var isHeading = oldItemInfo.IsHeading;
#pragma warning restore CS0618

                info.SetCollectionItemInfo(CollectionItemInfo.Obtain(
                    rowIndex,
                    oldItemInfo.RowSpan,
                    columnIndex,
                    oldItemInfo.ColumnSpan,
                    isHeading,
                    oldItemInfo.IsSelected));
            }
        }
    }
}
