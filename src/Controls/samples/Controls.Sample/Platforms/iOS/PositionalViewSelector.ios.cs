using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui
{
	internal partial class PositionalViewSelector
	{
		readonly LRUCache<(int sectionIndex, int itemIndex), PositionInfo> infoCache = new();

		public PositionInfo GetInfo(int sectionIndex, int itemIndex)
		{
			if (infoCache.TryGet((sectionIndex, itemIndex), out var cachedPositionInfo))
				return cachedPositionInfo;

			var positionInfo = GetUncachedInfo(sectionIndex, itemIndex);
			infoCache.AddReplace((sectionIndex, itemIndex), positionInfo);

			return positionInfo;
		}

		PositionInfo GetUncachedInfo(int sectionIndex, int itemIndex)
		{
			var realSectionIndex = sectionIndex;

			if (HasGlobalHeader)
			{
				if (sectionIndex == 0)
					return PositionInfo.ForHeader(0);

				// Global header takes up a section, real adapter is 1 less
				realSectionIndex--;
			}

			var realNumberOfSections = Adapter?.Sections ?? 0;

			if (HasGlobalFooter)
			{
				if (realSectionIndex >= realNumberOfSections)
					return PositionInfo.ForFooter(-1);
			}


			var realItemsInSection = Adapter?.ItemsForSection(realSectionIndex) ?? 0;

			var realItemIndex = itemIndex;

			var itemsAdded = 0;

			if (ViewSelector?.SectionHasHeader(realSectionIndex) ?? false)
			{
				itemsAdded++;
				realItemIndex--;

				if (itemIndex == 0)
					return PositionInfo.ForSectionHeader(-1, realSectionIndex, realItemsInSection);
			}

			if (ViewSelector.SectionHasFooter(realSectionIndex))
			{
				itemsAdded++;

				if (itemIndex >= realItemsInSection + itemsAdded - 1)
					return PositionInfo.ForSectionFooter(-1, realSectionIndex, realItemsInSection);
			}

			return PositionInfo.ForItem(-1, realSectionIndex, realItemIndex, CachedItemsForSection(realSectionIndex), realNumberOfSections);
		}
	}
}
