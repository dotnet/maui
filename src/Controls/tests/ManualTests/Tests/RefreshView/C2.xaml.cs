using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.RefreshView;

[Test(
	id: "C2",
	title: "RefreshView with CollectionView child.",
	category: Category.RefreshView)]
public partial class C2 : ContentPage
{
	public C2()
	{
		InitializeComponent();
		BindingContext = new MonkeysViewModel();
	}
}
