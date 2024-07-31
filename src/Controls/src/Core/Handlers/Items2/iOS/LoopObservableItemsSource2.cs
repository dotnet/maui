#nullable disable
using System;
using System.Collections;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal class LoopObservableItemsSource2 : Items.ObservableItemsSource, Items.ILoopItemsViewSource
	{
		//const int LoopBy = 3;

		public LoopObservableItemsSource2(IEnumerable itemSource, UICollectionViewController collectionViewController, bool loop, int group = -1) : base(itemSource, collectionViewController, group)
		{
			Loop = loop;
		}

		public bool Loop { get; set; }

		//We are going to add 2 items since we are inserting 1 item at the beginning and 1 item at the end
		public int LoopCount => Loop ? Count + 2 : Count;
	}
}
