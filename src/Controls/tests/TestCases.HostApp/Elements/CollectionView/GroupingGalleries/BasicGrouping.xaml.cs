using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	[Preserve(AllMembers = true)]
	public partial class BasicGrouping : ContentPage
	{
		public BasicGrouping()
		{
			InitializeComponent();

			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}