using Microsoft.Maui.ManualTests.Categories;
using Microsoft.Maui.ManualTests.ViewModels;

namespace Microsoft.Maui.ManualTests.Tests.CarouselView;

[Test(
	id: "M2",
	title: "Resize and Rotate",
	category: Category.CarouselView)]
public partial class M2_ResizeRotate : ContentPage
{
	private double width = 0;
	private double height = 0;
	public M2_ResizeRotate()
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
