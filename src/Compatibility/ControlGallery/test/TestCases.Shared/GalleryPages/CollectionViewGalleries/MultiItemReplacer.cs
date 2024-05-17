using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal class MultiItemReplacer : MultiTestObservableCollectionModifier
	{
		private readonly bool _withIndex;

		public MultiItemReplacer(CollectionView cv, bool withIndex = false) : base(cv, "Replace w/ 4 Items")
		{
			Entry.Keyboard = Keyboard.Default;
			_withIndex = withIndex;
		}

		protected override bool ParseIndexes(out int[] indexes)
		{
			return IndexParser.ParseIndexes(Entry.Text, 2, out indexes);
		}

		protected override string InitialEntryText => "1,3";

		protected override string LabelText => "Indexes (start, end):";

		protected override void ModifyObservableCollection(MultiTestObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			if (indexes.Length < 2)
			{
				return;
			}

			var index1 = indexes[0];
			var index2 = indexes[1];

			if (index1 > -1 && index2 < observableCollection.Count && index1 <= index2)
			{
				var newItems = new List<CollectionViewGalleryTestItem>();

				for (int n = 0; n < 4; n++)
				{
					newItems.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
						$"Added", "coffee.png", n));
				}

				if (_withIndex)
				{
					observableCollection.TestReplaceWithListAndIndex(index1, index2 - index1 + 1, newItems);
				}
				else
				{
					observableCollection.TestReplaceWithList(index1, index2 - index1 + 1, newItems);
				}
			}
		}
	}
}