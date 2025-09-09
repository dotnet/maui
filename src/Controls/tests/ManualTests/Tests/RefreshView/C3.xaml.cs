using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "C3",
	title: "RefreshView with ListView child.",
	category: Category.RefreshView)]
public partial class C3 : ContentPage
{
	public C3()
	{
		InitializeComponent();
		BindingContext = new MonkeysViewModel();
	}
}
