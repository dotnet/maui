using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class ItemMover : ObservableCollectionModifier
	{
		public ItemMover(CollectionView cv) : base(cv, "Move")
		{
			Entry.Keyboard = Keyboard.Default;
		}

		protected override bool ParseIndexes(out int[] indexes)
		{
			return IndexParser.ParseIndexes(Entry.Text, 2, out indexes);
		}

		protected override string InitialEntryText => "1,3";

		protected override string LabelText => "Indexes (from, to):";

		protected override void ModifyObservableCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			if (indexes.Length < 2)
			{
				return;
			}

			var index1 = indexes[0];
			var index2 = indexes[1];

			if (index1 != index2 && index1 > -1 && index2 > -1 && index1 < observableCollection.Count &&
				index2 < observableCollection.Count)
			{
				observableCollection.Move(index1, index2);
			}
		}
	}
}