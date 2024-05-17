namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal class MultiItemRemover : MultiTestObservableCollectionModifier
	{
		readonly bool _withIndex;

		public MultiItemRemover(CollectionView cv, bool withIndex = false) : base(cv, "Remove")
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
				if (_withIndex)
				{
					observableCollection.TestRemoveWithListAndIndex(index1, (index2 - index1) + 1);
				}
				else
				{
					observableCollection.TestRemoveWithList(index1, (index2 - index1) + 1);
				}
			}
		}
	}
}