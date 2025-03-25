namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal abstract class MultiTestObservableCollectionModifier : CollectionModifier
	{
		protected MultiTestObservableCollectionModifier(CollectionView cv, string buttonText) : base(cv, buttonText)
		{
		}

		protected override void OnButtonClicked()
		{
			if (!ParseIndexes(out int[] indexes))
			{
				return;
			}

			if (_cv.ItemsSource is MultiTestObservableCollection<CollectionViewGalleryTestItem> observableCollection)
			{
				ModifyObservableCollection(observableCollection, indexes);
			}
		}

		protected abstract void ModifyObservableCollection(MultiTestObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes);
	}
}