using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.CollectionViewGalleries.GroupingGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class GridGrouping : ContentPage
	{
		public GridGrouping()
		{
			InitializeComponent();
			CollectionView.ItemsSource = new SuperTeams();
		}
	}
}