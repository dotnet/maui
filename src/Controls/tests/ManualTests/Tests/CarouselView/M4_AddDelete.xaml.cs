using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;
using Microsoft.Maui.ManualTests.Models;

namespace Microsoft.Maui.ManualTests.Tests.CarouselView;

[Test(
	id: "M4",
	title: "Add/Delete",
	category: Category.CarouselView)]
public partial class M4_AddDelete : ContentPage
{
	private double width = 0;
	private double height = 0;
	private MonkeysViewModel _viewModel;

	public M4_AddDelete()
	{
		InitializeComponent();
		_viewModel = new MonkeysViewModel();
		BindingContext = _viewModel;
	}

	private async void OnAddItemClicked(object sender, EventArgs e)
	{
		if (_viewModel?.Monkeys != null)
		{
			await Navigation.PushAsync(new AddMonkeyPage(_viewModel.Monkeys));
		}
	}

	private async void OnImageTapped(object sender, EventArgs e)
	{
		if (sender is View view && view.BindingContext is Monkey monkey)
		{
			var viewModel = BindingContext as MonkeysViewModel;
			await Navigation.PushAsync(new MonkeyDetailPage(monkey, viewModel?.Monkeys));
		}
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
		if (width != this.width || height != this.height)
		{
			this.width = width;
			this.height = height;
			if (width > height)
			{
				carousel.HeightRequest = 200;
				carousel.WidthRequest = width - 100;
			}
			else
			{
				carousel.HeightRequest = height - 300;
				carousel.WidthRequest = 350;
			}
		}
	}
}
