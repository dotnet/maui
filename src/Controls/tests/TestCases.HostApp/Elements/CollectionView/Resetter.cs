namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class Resetter : MultiTestObservableCollectionModifier
	{
		public Resetter(CollectionView cv) : base(cv, "Reset")
		{
		}

		protected override void ModifyObservableCollection(MultiTestObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes)
		{
			observableCollection.TestReset();
		}
	}
}