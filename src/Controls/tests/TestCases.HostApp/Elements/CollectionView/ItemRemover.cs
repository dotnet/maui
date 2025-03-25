using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class ItemRemover : ObservableCollectionModifier
	{
		public ItemRemover(CollectionView cv) : base(cv, "Remove")
		{
		}

		protected override void ModifyObservableCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			var index = indexes[0];

			if (index > -1 && index < observableCollection.Count)
			{
				observableCollection.Remove(observableCollection[index]);
			}
		}
	}
}