namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	public partial class SwitchGrouping : ContentPage
	{
		public SwitchGrouping()
		{
			InitializeComponent();

			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}