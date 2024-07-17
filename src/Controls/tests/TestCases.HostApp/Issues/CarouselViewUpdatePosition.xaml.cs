using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	// Issue11224 (src\ControlGallery\src\Issues.Shared\Issue11224.cs
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.None, 11224, "[Bug] CarouselView Position property fails to update visual while control isn't visible", PlatformAffected.Android)]
	public partial class CarouselViewUpdatePosition : ContentPage
	{
		public CarouselViewUpdatePosition()
		{
			Title = "Issue 11224";

			InitializeComponent();

			carousel.Scrolled += (sender, args) =>
			{
				if (args.CenterItemIndex == 3)
					ResultLabel.Text = "The test has passed";
				else
					ResultLabel.Text = "The test has failed";
			};

			carousel.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == CarouselView.IsVisibleProperty.PropertyName)
				{
					if (carousel.IsVisible && carousel.Position == 3)
					{
						ResultLabel.Text = "The test has passed";
					}
				}
			};
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			carousel.IsVisible = true;
		}
	}
}