namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	public partial class MeasureFirstStrategy : ContentPage
	{
		public MeasureFirstStrategy()
		{
			InitializeComponent();

			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}