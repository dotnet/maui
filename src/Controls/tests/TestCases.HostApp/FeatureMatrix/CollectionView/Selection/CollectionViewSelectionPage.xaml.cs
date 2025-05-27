using System;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class CollectionViewSelectionPage : ContentPage
	{
		private CollectionViewViewModel _viewModel;

		public CollectionViewSelectionPage()
		{
			InitializeComponent();
			_viewModel = new CollectionViewViewModel();
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new CollectionViewViewModel();
			_viewModel.ItemsSourceType = ItemsSourceType.ObservableCollection5T;
			await Navigation.PushAsync(new SelectionOptionsPage(_viewModel));
		}

		void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (BindingContext is CollectionViewViewModel vm)
			{
				vm.SelectionChangedEventCount++;

				var previousSelection = e.PreviousSelection.Any()
					? string.Join(", ", e.PreviousSelection.OfType<CollectionViewViewModel.CollectionViewTestItem>().Select(item => item.Caption))
					: "No previous items";

				var currentSelection = e.CurrentSelection.Any()
					? string.Join(", ", e.CurrentSelection.OfType<CollectionViewViewModel.CollectionViewTestItem>().Select(item => item.Caption))
					: "No current items";
				vm.PreviousSelectionText = $"{previousSelection}";
				vm.CurrentSelectionText = $"{currentSelection}";
			}
		}
	}
}