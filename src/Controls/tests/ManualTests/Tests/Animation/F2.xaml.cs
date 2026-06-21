using Microsoft.Maui.ManualTests.Categories;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "F2",
	title: "Animation tasks do not complete when Remove Animations enabled.",
	category: Category.Animation)]
public partial class F2 : ContentPage
{
	public F2()
	{
		InitializeComponent();

		Appearing += async (sender, args) =>
		{
			await Label1.FadeTo(1, 120000);
			Label2.IsVisible = true;
		};
	}
}
