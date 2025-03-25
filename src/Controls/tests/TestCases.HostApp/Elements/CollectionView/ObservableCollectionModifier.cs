using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal abstract class ObservableCollectionModifier : CollectionModifier
	{
		protected ObservableCollectionModifier(CollectionView cv, string buttonText) : base(cv, buttonText)
		{
		}

		protected override void OnButtonClicked()
		{
			if (!ParseIndexes(out int[] indexes))
			{
				return;
			}

			if (_cv.ItemsSource is ObservableCollection<CollectionViewGalleryTestItem> observableCollection)
			{
				ModifyObservableCollection(observableCollection, indexes);
			}
		}

		protected abstract void ModifyObservableCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes);
	}
}