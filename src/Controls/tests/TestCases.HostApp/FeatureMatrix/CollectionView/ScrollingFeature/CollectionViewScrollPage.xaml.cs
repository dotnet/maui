using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
namespace Maui.Controls.Sample;
public partial class CollectionViewScrollPage : ContentPage
{
	private CollectionViewViewModel _viewModel;
	public CollectionViewScrollPage()
	{
		InitializeComponent();
		_viewModel = new CollectionViewViewModel();
		_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT3;
		BindingContext = _viewModel;
	}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new CollectionViewViewModel();
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollectionT3;
			await Navigation.PushAsync(new ScrollBehaviorOptionsPage(_viewModel));
		}
}