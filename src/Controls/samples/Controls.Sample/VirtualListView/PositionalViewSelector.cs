using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	internal partial class PositionalViewSelector
	{
		public readonly IVirtualListView VirtualListView;
		public IVirtualListViewAdapter Adapter => VirtualListView?.Adapter;
		public IVirtualListViewSelector ViewSelector => VirtualListView?.ViewSelector;
		public bool HasGlobalHeader => VirtualListView?.Header != null;
		public bool HasGlobalFooter => VirtualListView?.Footer != null;

		public PositionalViewSelector(IVirtualListView virtualListView)
		{
			VirtualListView = virtualListView;
		}

		const string GlobalHeaderReuseId = "GLOBAL_HEADER";
		const string GlobalFooterReuseId = "GLOBAL_FOOTER";

		readonly Dictionary<int, int> cachedItemsInSection = new ();

		int CachedItemsForSection(int sectionIndex)
		{
			if (cachedItemsInSection.TryGetValue(sectionIndex, out var n))
				return n;

			n = Adapter.ItemsForSection(sectionIndex);
			cachedItemsInSection.TryAdd(sectionIndex, n);
			return n;
		}

		public void Reset()
		{
			infoCache.Clear();
			cachedItemsInSection.Clear();
			cachedTotalCount = null;
		}

		int? cachedTotalCount;
		public int TotalCount
		{
			get
			{
				if (!cachedTotalCount.HasValue)
				{
					var tc = GetTotalCount();
					if (tc > 0)
						cachedTotalCount = tc;
				}

				return cachedTotalCount ?? 0;
			}
		}

		int GetTotalCount()
		{
			if (Adapter == null)
				return 0;

			var sum = 0;

			if (HasGlobalHeader)
				sum += 1;

			if (Adapter != null)
			{
				for (int s = 0; s < Adapter.Sections; s++)
				{
					if (ViewSelector.SectionHasHeader(s))
						sum += 1;

					sum += CachedItemsForSection(s);

					if (ViewSelector.SectionHasFooter(s))
						sum += 1;
				}
			}

			if (HasGlobalFooter)
				sum += 1;

			return sum;
		}

#if !IOS && !MACCATALYST
		readonly LRUCache<int, PositionInfo> infoCache = new();

		public PositionInfo GetInfo(int position)
		{
			if (infoCache.TryGet(position, out var cachedPositionInfo))
				return cachedPositionInfo;

			var positionInfo = GetUncachedInfo(position);

			infoCache.AddReplace(position, positionInfo);
			return positionInfo;
		}

		PositionInfo GetUncachedInfo(int position)
		{
			if (Adapter == null)
				return null;

			var linear = 0;

			var numberSections = Adapter.Sections;

			if (HasGlobalHeader)
			{
				if (position == 0)
					return PositionInfo.ForHeader(position);

				linear++;
			}

			for (int s = 0; s < numberSections; s++)
			{
				if (ViewSelector.SectionHasHeader(s))
				{
					if (position == linear)
						return PositionInfo.ForSectionHeader(position, s);

					linear++;
				}

				var itemsInSection = CachedItemsForSection(s);

				// It's an item in the section, return it for this item
				if (position < linear + itemsInSection)
				{
					var itemIndex = position - linear;

					return PositionInfo.ForItem(position, s, itemIndex, itemsInSection, numberSections);
				}

				linear += itemsInSection;

				if (ViewSelector.SectionHasFooter(s))
				{
					if (position == linear)
						return PositionInfo.ForSectionFooter(position, s);

					linear++;
				}
			}

			return new PositionInfo
			{
				Position = position,
				Kind = PositionKind.Footer
			};
		}
#endif

	}
}
