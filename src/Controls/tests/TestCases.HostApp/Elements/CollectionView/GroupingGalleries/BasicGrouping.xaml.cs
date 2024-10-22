namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{

	public partial class BasicGrouping : ContentPage
	{
		public BasicGrouping()
		{
			InitializeComponent();

			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}