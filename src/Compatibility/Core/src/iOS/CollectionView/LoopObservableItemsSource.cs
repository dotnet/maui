//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System.Collections;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class LoopObservableItemsSource : ObservableItemsSource, ILoopItemsViewSource
	{
		const int LoopBy = 3;

		public LoopObservableItemsSource(IEnumerable itemSource, UICollectionViewController collectionViewController, bool loop, int group = -1) : base(itemSource, collectionViewController, group)
		{
			Loop = loop;
		}

		public bool Loop { get; set; }

		public int LoopCount => Loop ? Count * LoopBy : Count;

		protected override NSIndexPath[] CreateIndexesFrom(int startIndex, int count)
		{
			if (!Loop)
			{
				return base.CreateIndexesFrom(startIndex, count);
			}

			return IndexPathHelpers.GenerateLoopedIndexPathRange(Section,
				(int)CollectionView.NumberOfItemsInSection(Section), LoopBy, startIndex, count);
		}
	}
}
