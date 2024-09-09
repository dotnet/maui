using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class ItemReplacer : ObservableCollectionModifier
	{
		public ItemReplacer(CollectionView cv) : base(cv, "Replace")
		{
		}

		protected override void ModifyObservableCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			var index = indexes[0];

			if (index > -1 && index < observableCollection.Count)
			{
				var replacement = new CollectionViewGalleryTestItem(DateTime.Now, "Replacement", "coffee.png", index);
				observableCollection[index] = replacement;
			}
		}
	}
}