namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 13258, "MAUI Slider thumb image is big on android", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
	public class Issue13258 : TestContentPage
	{
		protected override void Init()
		{
			StackLayout rootLayout = new StackLayout() { Spacing = 10, Padding = 10 };

			Label slider1DescriptionLabel = CreateLabel("Slider with Thumb Image");
			Slider slider1 = CreateSlider("avatar.png");

			Label slider2DescriptionLabel = CreateLabel("Thumb Image will be set to coffee.png at run time");
			Slider slider2 = CreateSlider();

			Label slider3DescriptionLabel = CreateLabel("Thumb Image will be set to null at run time");
			Slider slider3 = CreateSlider("shopping_cart.png");

			Button button = new Button() { Text = "Change Thumb Image", AutomationId = "ToggleImageBtn" };
			button.Clicked += (s, e) => ToggleThumbImages(slider2, slider3);

			rootLayout.Children.Add(slider1DescriptionLabel);
			rootLayout.Children.Add(slider1);

			rootLayout.Children.Add(slider2DescriptionLabel);
			rootLayout.Children.Add(slider2);

			rootLayout.Children.Add(slider3DescriptionLabel);
			rootLayout.Children.Add(slider3);

			rootLayout.Children.Add(button);

			Content = rootLayout;
		}

		Label CreateLabel(string text)
		{
			return new Label { Text = text };
		}

		Slider CreateSlider(string thumbImageSource = null)
		{
			return new Slider { ThumbImageSource = thumbImageSource };
		}

		private void ToggleThumbImages(Slider slider2, Slider slider3)
		{
			slider2.ThumbImageSource = "coffee.png";
			slider3.ThumbImageSource = null;
		}
	}
}
