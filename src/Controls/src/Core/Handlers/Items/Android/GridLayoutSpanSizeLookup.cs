#nullable disable
using System;
using System.Collections.Generic;
using AndroidX.RecyclerView.Widget;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	internal class GridLayoutSpanSizeLookup : GridLayoutManager.SpanSizeLookup
	{
		readonly GridItemsLayout _gridItemsLayout;
		readonly RecyclerView _recyclerView;

		// Each block is either a single full-span item or a run of regular items between two full-span items.
		// Runs always start on a fresh row, so span index/row inside a run is plain arithmetic.
		readonly List<Block> _blocks = new List<Block>();
		RecyclerView.Adapter _observedAdapter;
		InvalidatingObserver _observer;
		int _blocksSpanCount = -1;
		bool _blocksDirty = true;

		public GridLayoutSpanSizeLookup(GridItemsLayout gridItemsLayout, RecyclerView recyclerView)
		{
			_gridItemsLayout = gridItemsLayout;
			_recyclerView = recyclerView;

			SpanIndexCacheEnabled = true;
			SpanGroupIndexCacheEnabled = true;
		}

		public override int GetSpanSize(int position)
		{
			var adapter = _recyclerView.GetAdapter();

			// EmptyViewAdapter uses private incrementing view type IDs that never match
			// the static ItemViewType constants. All items it contains (header, empty view,
			// footer) should span the full grid width.
			if (adapter is EmptyViewAdapter)
			{
				return _gridItemsLayout.Span;
			}

			var itemViewType = adapter.GetItemViewType(position);

			if (IsFullSpan(itemViewType))
			{
				return _gridItemsLayout.Span;
			}

			return 1;
		}

		// The base implementations resolve an uncached position by calling GetSpanSize (a JNI round-trip
		// into managed code) for every position between the nearest cached key and the target. For large
		// sources that is O(n) per lookup, paid on every ScrollTo into a cold region and every row laid out
		// while scrolling backwards. The overrides below answer from the items source instead.
		public override int GetSpanIndex(int position, int spanCount)
		{
			if (!TryGetBlock(position, spanCount, out var block, out var indexInBlock))
			{
				return base.GetSpanIndex(position, spanCount);
			}

			return block.IsFullSpan ? 0 : indexInBlock % spanCount;
		}

		public override int GetSpanGroupIndex(int adapterPosition, int spanCount)
		{
			if (!TryGetBlock(adapterPosition, spanCount, out var block, out var indexInBlock))
			{
				return base.GetSpanGroupIndex(adapterPosition, spanCount);
			}

			return block.IsFullSpan ? block.FirstRow : block.FirstRow + indexInBlock / spanCount;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				StopObserving();
			}

			base.Dispose(disposing);
		}

		static bool IsFullSpan(int itemViewType)
		{
			return itemViewType == ItemViewType.Header || itemViewType == ItemViewType.Footer
				|| itemViewType == ItemViewType.GroupHeader || itemViewType == ItemViewType.GroupFooter;
		}

		bool TryGetBlock(int position, int spanCount, out Block block, out int indexInBlock)
		{
			block = default;
			indexInBlock = 0;

			if (position < 0 || spanCount < 1)
			{
				return false;
			}

			var adapter = _recyclerView.GetAdapter();

			if (adapter is EmptyViewAdapter)
			{
				// Every item is full-span, one per row.
				block = new Block(position, 1, isFullSpan: true, firstRow: position);
				return true;
			}

			if (adapter is not IItemsViewAdapter itemsViewAdapter)
			{
				return false;
			}

			if (!ReferenceEquals(adapter, _observedAdapter))
			{
				StartObserving(adapter);
			}

			if (_blocksDirty || _blocksSpanCount != spanCount)
			{
				if (!RebuildBlocks(itemsViewAdapter.ItemsSource, spanCount))
				{
					return false;
				}
			}

			var blockIndex = FindBlock(position);

			if (blockIndex < 0)
			{
				return false;
			}

			block = _blocks[blockIndex];
			indexInBlock = position - block.Start;
			return true;
		}

		bool RebuildBlocks(IItemsViewSource source, int spanCount)
		{
			_blocks.Clear();

			var builder = new BlockBuilder(_blocks, spanCount);

			switch (source)
			{
				case ObservableGroupedSource grouped:
					builder.AddFullSpan(grouped.HasHeader);

					for (int g = 0; g < grouped.GroupCount; g++)
					{
						var group = grouped.GetGroupItemsViewSource(g);
						var itemCount = group.Count - (group.HasHeader ? 1 : 0) - (group.HasFooter ? 1 : 0);

						builder.AddFullSpan(group.HasHeader);
						builder.AddRun(itemCount);
						builder.AddFullSpan(group.HasFooter);
					}

					builder.AddFullSpan(grouped.HasFooter);
					break;

				case IItemsViewSource flat:
					builder.AddFullSpan(flat.HasHeader);
					builder.AddRun(flat.Count - (flat.HasHeader ? 1 : 0) - (flat.HasFooter ? 1 : 0));
					builder.AddFullSpan(flat.HasFooter);
					break;

				default:
					_blocksDirty = true;
					return false;
			}

			_blocksSpanCount = spanCount;
			_blocksDirty = false;
			return true;
		}

		int FindBlock(int position)
		{
			int low = 0;
			int high = _blocks.Count - 1;

			while (low <= high)
			{
				int mid = (low + high) >> 1;
				var block = _blocks[mid];

				if (position < block.Start)
				{
					high = mid - 1;
				}
				else if (position >= block.Start + block.Count)
				{
					low = mid + 1;
				}
				else
				{
					return mid;
				}
			}

			return -1;
		}

		void StartObserving(RecyclerView.Adapter adapter)
		{
			StopObserving();

			_observer ??= new InvalidatingObserver(this);
			adapter.RegisterAdapterDataObserver(_observer);
			_observedAdapter = adapter;
			_blocksDirty = true;
		}

		void StopObserving()
		{
			// The previous adapter may already be disposed after an adapter swap.
			if (_observedAdapter is { Handle: not 0 } && _observer is not null)
			{
				_observedAdapter.UnregisterAdapterDataObserver(_observer);
			}

			_observedAdapter = null;
		}

		readonly struct Block
		{
			public Block(int start, int count, bool isFullSpan, int firstRow)
			{
				Start = start;
				Count = count;
				IsFullSpan = isFullSpan;
				FirstRow = firstRow;
			}

			public int Start { get; }
			public int Count { get; }
			public bool IsFullSpan { get; }
			public int FirstRow { get; }
		}

		struct BlockBuilder
		{
			readonly List<Block> _blocks;
			readonly int _spanCount;
			int _nextPosition;
			int _nextRow;

			public BlockBuilder(List<Block> blocks, int spanCount)
			{
				_blocks = blocks;
				_spanCount = spanCount;
				_nextPosition = 0;
				_nextRow = 0;
			}

			public void AddFullSpan(bool present)
			{
				if (!present)
				{
					return;
				}

				_blocks.Add(new Block(_nextPosition, 1, isFullSpan: true, firstRow: _nextRow));
				_nextPosition += 1;
				_nextRow += 1;
			}

			public void AddRun(int itemCount)
			{
				if (itemCount <= 0)
				{
					return;
				}

				// Adjacent groups without a header/footer between them share rows, so their
				// items form one run as far as GridLayoutManager is concerned.
				if (_blocks.Count > 0 && !_blocks[_blocks.Count - 1].IsFullSpan)
				{
					var previous = _blocks[_blocks.Count - 1];
					itemCount += previous.Count;
					_blocks.RemoveAt(_blocks.Count - 1);
					_nextPosition = previous.Start;
					_nextRow = previous.FirstRow;
				}

				_blocks.Add(new Block(_nextPosition, itemCount, isFullSpan: false, firstRow: _nextRow));
				_nextPosition += itemCount;
				_nextRow += (itemCount + _spanCount - 1) / _spanCount;
			}
		}

		// GridLayoutManager already invalidates its own span caches on these notifications; this only
		// marks the block table stale so it is rebuilt from the live source on the next lookup.
		class InvalidatingObserver : RecyclerView.AdapterDataObserver
		{
			readonly GridLayoutSpanSizeLookup _owner;

			public InvalidatingObserver(GridLayoutSpanSizeLookup owner)
			{
				_owner = owner;
			}

			public override void OnChanged() => _owner._blocksDirty = true;
			public override void OnItemRangeChanged(int positionStart, int itemCount) => _owner._blocksDirty = true;
			public override void OnItemRangeChanged(int positionStart, int itemCount, Java.Lang.Object payload) => _owner._blocksDirty = true;
			public override void OnItemRangeInserted(int positionStart, int itemCount) => _owner._blocksDirty = true;
			public override void OnItemRangeRemoved(int positionStart, int itemCount) => _owner._blocksDirty = true;
			public override void OnItemRangeMoved(int fromPosition, int toPosition, int itemCount) => _owner._blocksDirty = true;
		}
	}
}
