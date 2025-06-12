using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "C1",
	title: "RefreshView with ScrollView child.",
	category: Category.RefreshView)]
public partial class C1 : ContentPage
{
	public C1()
	{
		InitializeComponent();
		BindingContext = new MonkeysViewModel();
	}
}
