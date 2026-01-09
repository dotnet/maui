using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.Models;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.ScrollView;

[Test(
	id: "N3",
	title: "Navigation",
	category: Category.ScrollView)]
public partial class N3_Navigation : ContentPage
{
	public Command<Monkey> NavigateCommand { get; }
	public Microsoft.Maui.ManualTests.ViewModels.MonkeysViewModel Monkeys { get; }
	public N3_Navigation()
	{
		InitializeComponent();
		BindingContext = new MonkeysViewModel();
	}
	private async void OnImageTapped(object sender, EventArgs e)
	{
		if (sender is View view && view.BindingContext is Monkey monkey)
		{
			await Navigation.PushAsync(new MonkeyDetailPage(monkey));
		}
	}
}
