using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 27417,
		"Button.Image behaviors differently on each platform and has extra padding even with no Text", PlatformAffected.All)]
	public class Bugzilla27417 : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label { Text = @"There should be 8 buttons below (the bottom 7 are in a ScrollView). 
Buttons 1 and 2 have images which are horizontally and vertically centered.
Button 3 should have the text 'Click Me' in the center.
Button 4 should have an image in the center and no text.
Button 5 should have the image on the left and the text on the right.
Button 6 should have the image on the top and the text on the bottom.
Button 7 should have the image on the bottom and the text on the top.
Button 8 have the image on the right and the text on the left." };

			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = 100 }
				},
				VerticalOptions = LayoutOptions.Start,
				Children =
				{
					new Button
					{
						HeightRequest = 500, // Making sure that the image still gets centered vertically even if the HeightRequest won't be honored
						Image = "coffee.png"
					}
				}
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				Spacing = 10,
				Children =
				{
					instructions,
					grid,
					new ScrollView
					{
						Content = new StackLayout
						{
							Spacing = 10,
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.Center,
							Children =
							{
								new Button { WidthRequest = 200, HeightRequest = 300, Image = "coffee.png" },
								new Button { Text = "Click Me", BackgroundColor = Color.Gray },
								new Button { Image = "coffee.png", BackgroundColor = Color.Gray },
								CreateButton(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Left, 10)),
								CreateButton(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Top, 10)),
								CreateButton(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Bottom, 10)),
								CreateButton(new Button.ButtonContentLayout(Button.ButtonContentLayout.ImagePosition.Right, 10))
							}
						}
					}
				}
			};
		}

		static Button CreateButton(Button.ButtonContentLayout layout)
		{
			return new Button
			{
				Text = "Click Me",
				Image = "coffee.png",
				ContentLayout = layout,
				BackgroundColor = Color.Gray
			};
		}
	}
}