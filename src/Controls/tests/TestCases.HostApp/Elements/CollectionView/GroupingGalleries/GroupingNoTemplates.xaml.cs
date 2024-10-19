namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	public partial class GroupingNoTemplates : ContentPage
	{
		public GroupingNoTemplates()
		{
			InitializeComponent();
			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}