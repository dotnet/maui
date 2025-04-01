namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	public partial class GridGrouping : ContentPage
	{
		public GridGrouping()
		{
			InitializeComponent();
			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}