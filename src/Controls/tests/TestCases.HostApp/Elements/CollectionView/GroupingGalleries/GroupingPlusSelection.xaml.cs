namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	public partial class GroupingPlusSelection : ContentPage
	{
		public GroupingPlusSelection()
		{
			InitializeComponent();
			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}