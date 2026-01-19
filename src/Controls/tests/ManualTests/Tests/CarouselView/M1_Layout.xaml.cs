using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;
using Microsoft.Maui.ManualTests.Models;

namespace Microsoft.Maui.ManualTests.Tests.CarouselView;

[Test(
	id: "M1",
	title: "Layout",
	category: Category.CarouselView)]
public partial class M1_Layout : ContentPage
{
	private double width = 0;
	private double height = 0;

	public M1_Layout()
	{
		InitializeComponent();
		BindingContext = new MonkeysViewModel();
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);
		if (width != this.width || height != this.height)
		{
			this.width = width;
			this.height = height;
			if (width > height)
			{
				carousel.HeightRequest = 200;
				carousel.WidthRequest = width - 100;
			}
			else
			{
				carousel.HeightRequest = height - 300;
				carousel.WidthRequest = 350;
			}
		}
	}
}
