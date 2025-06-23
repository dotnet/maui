namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class MultiItemMover : MultiTestObservableCollectionModifier
	{
		public MultiItemMover(CollectionView cv) : base(cv, "Move")
		{
			Entry.Keyboard = Keyboard.Default;
		}

		protected override bool ParseIndexes(out int[] indexes)
		{
			return IndexParser.ParseIndexes(Entry.Text, 3, out indexes);
		}

		protected override string InitialEntryText => "0,3,5";

		protected override string LabelText => "Indexes (start, end, destination):";

		protected override void ModifyObservableCollection(MultiTestObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			if (indexes.Length < 3)
			{
				return;
			}

			var startIndex = indexes[0];
			var endIndex = indexes[1];
			var destinationIndex = indexes[2];

			var count = observableCollection.Count;

			// -1 < startIndex < endIndex < count
			if (startIndex < 0 || endIndex >= count || endIndex <= startIndex)
			{
				return;
			}

			var itemsToMove = endIndex - startIndex;

			// Can't move the items past the end of the list
			if (destinationIndex > (count - itemsToMove))
			{
				return;
			}

			observableCollection.TestMoveWithList(startIndex, (endIndex - startIndex) + 1, destinationIndex);
		}
	}
}