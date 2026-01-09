using Microsoft.Maui.ManualTests.Models;

namespace Microsoft.Maui.ManualTests.Tests.ScrollView;

public partial class MonkeyDetailPage : ContentPage
{
	public MonkeyDetailPage(Monkey monkey)
	{
		InitializeComponent();
		BindingContext = monkey;
	}

	private async void OnBackClicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}
}
