using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ItemRemover : CollectionModifier 
	{
		public ItemRemover(CollectionView cv) : base(cv, "Remove")
		{
		}

		protected override void ModifyCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			var index = indexes[0];

			if (index > -1 && index < observableCollection.Count)
			{
				observableCollection.Remove(observableCollection[index]);
			}
		}
	}
}