using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

// More information: https://learn.microsoft.com/en-us/dotnet/maui/ios/platform-specifics/flyoutpage-shadow
[Test(
	id: "H1",
	title: "FlyoutPage Shadow.",
	category: Category.FlyoutPage)]
public partial class H1 : FlyoutPage
{
	public H1()
	{
		InitializeComponent();
		btn.Clicked += async (s, e) => await Navigation.PopModalAsync(false);
	}
}