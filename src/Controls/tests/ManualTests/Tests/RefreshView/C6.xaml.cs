using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "C6",
	title: "RefreshView is disabled correctly.",
	category: Category.RefreshView)]
public partial class C6 : ContentPage
{
	public C6()
	{
		InitializeComponent();

		BindingContext = new MonkeysViewModel();
	}
}
