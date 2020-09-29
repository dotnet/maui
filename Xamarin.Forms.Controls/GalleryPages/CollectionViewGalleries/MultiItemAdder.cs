using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class MultiItemAdder : MultiTestObservableCollectionModifier
	{
		readonly bool _withIndex;

		public MultiItemAdder(CollectionView cv, bool withIndex = false) : base(cv, "Add 4 Items")
		{
			_withIndex = withIndex;
		}

		protected override void ModifyObservableCollection(MultiTestObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			var index1 = indexes[0];

			if (index1 > -1 && index1 < observableCollection.Count)
			{
				var newItems = new List<CollectionViewGalleryTestItem>();

				for (int n = 0; n < 4; n++)
				{
					newItems.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
						$"Added", "coffee.png", n));
				}

				if (_withIndex)
				{
					observableCollection.TestAddWithListAndIndex(newItems, index1);
				}
				else
				{
					observableCollection.TestAddWithList(newItems, index1);
				}
			}
		}
	}
}