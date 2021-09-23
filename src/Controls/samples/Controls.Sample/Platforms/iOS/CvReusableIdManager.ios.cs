using System;
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	internal class ReusableIdManager
	{
		public ReusableIdManager(string uniquePrefix, NSString supplementaryKind = null)
		{
			UniquePrefix = uniquePrefix;
			SupplementaryKind = supplementaryKind;
			managedIds = new List<string>();
			lockObj = new object();
		}

		public string UniquePrefix { get; }
		public NSString SupplementaryKind { get; }

		readonly List<string> managedIds;
		readonly object lockObj;

		NSString GetReuseId(int i, string idModifier = null)
			=> new NSString($"_{UniquePrefix}_VirtualListView_{i}");

		public NSString GetReuseId(UICollectionView collectionView, string managedId)
		{
			var viewType = 0;

			lock (lockObj)
			{
				viewType = managedIds.IndexOf(managedId);

				if (viewType < 0)
				{
					managedIds.Add(managedId);
					viewType = managedIds.Count - 1;

					collectionView.RegisterClassForCell(
						typeof(CvCell),
						GetReuseId(viewType));
				}
			}

			return GetReuseId(viewType);
		}

		public void ResetTemplates(UICollectionView collectionView)
		{
			lock (lockObj)
			{
				for (int i = 0; i < managedIds.Count; i++)
					collectionView.RegisterClassForCell(null, GetReuseId(i));

				managedIds.Clear();
			}
		}
	}
}
