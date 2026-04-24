using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.BugFixes;

[Test(
	id: "L3",
	title: "Navigation.PushAsync",
	category: Category.BugFixes)]
public partial class L3_Navigation_PushAsync : ContentPage
{
	public L3_Navigation_PushAsync()
	{
		InitializeComponent();
	}

	private async void OnNavigateToNewPage1Clicked(object sender, EventArgs e)
	{
		await Navigation.PushAsync(new ContentPage
		{
			Title = "New Page1",
			Content = new Label
			{
				Text = "Welcome to New Page 1",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			}
		});
	}

}