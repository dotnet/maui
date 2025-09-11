using Microsoft.Maui.ManualTests.Tests;

namespace Microsoft.Maui.ManualTests.Categories;

public partial class CategoryPage : ContentPage
{
	public CategoryPage(CategoryViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}

	private void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is TestPageInfo test)
		{
			var page = (Page)Activator.CreateInstance(test.Type);

			if (page is ContentPage)
				Navigation.PushAsync(page);
			else if (page is not Shell)
				Navigation.PushModalAsync(page);
			else
				this.Window.Page = page;
		}

		var cv = sender as CollectionView;
		if (cv is not null)
			cv.SelectedItem = null;
	}
}
