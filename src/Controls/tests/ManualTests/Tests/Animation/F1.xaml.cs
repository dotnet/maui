using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "F1",
	title: "Animation tasks do not complete when Battery Saver enabled.",
	category: Category.Animation)]
public partial class F1 : ContentPage
{
	public F1()
	{
		InitializeComponent();

		Appearing += async (sender, args) =>
		{
			await Label1.FadeTo(1, 120000);
			Label2.IsVisible = true;
		};
	}
}
