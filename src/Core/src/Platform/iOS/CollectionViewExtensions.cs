// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class CollectionViewExtensions
	{
		public static void UpdateVerticalScrollBarVisibility(this UICollectionView collectionView, ScrollBarVisibility scrollBarVisibility)
		{
			collectionView.ShowsVerticalScrollIndicator = scrollBarVisibility == ScrollBarVisibility.Always || scrollBarVisibility == ScrollBarVisibility.Default;
		}

		public static void UpdateHorizontalScrollBarVisibility(this UICollectionView collectionView, ScrollBarVisibility scrollBarVisibility)
		{
			collectionView.ShowsHorizontalScrollIndicator = scrollBarVisibility == ScrollBarVisibility.Always || scrollBarVisibility == ScrollBarVisibility.Default;
		}
	}
}
