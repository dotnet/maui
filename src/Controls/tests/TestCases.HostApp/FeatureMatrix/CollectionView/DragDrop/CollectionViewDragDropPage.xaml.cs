using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class CollectionViewDragDropPage : ContentPage
{
	private CollectionViewViewModel _viewModel;

	public CollectionViewDragDropPage()
	{
		InitializeComponent();
		BindingContext = _viewModel = CreateDefaultViewModel();
	}

	private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
	{
		BindingContext = _viewModel = CreateDefaultViewModel();
		await Navigation.PushAsync(new DragDropOptionsPage(_viewModel));
	}

	static CollectionViewViewModel CreateDefaultViewModel()
	{
		var viewModel = new CollectionViewViewModel
		{
			ItemsSourceType = ItemsSourceType.GroupedListT,
			IsGrouped = true,
			CanReorderItems = true,
			CanMixGroups = true,
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
		};

		return viewModel;
	}
}
