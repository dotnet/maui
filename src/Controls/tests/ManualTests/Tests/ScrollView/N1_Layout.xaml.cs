using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.ScrollView;

[Test(
	id: "N1",
	title: "Layout",
	category: Category.ScrollView)]
public partial class N1_Layout : ContentPage
{
	public N1_Layout()
	{
		InitializeComponent();
		BindingContext = new MonkeysViewModel();
	}
}
