using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.Models;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.ScrollView;

[Test(
	id: "N2",
	title: "Resize and Rotate",
	category: Category.ScrollView)]
public partial class N2_ResizeRotate : ContentPage
{
	public N2_ResizeRotate()
	{
		InitializeComponent();
		BindingContext = new MonkeysViewModel();
	}
}
