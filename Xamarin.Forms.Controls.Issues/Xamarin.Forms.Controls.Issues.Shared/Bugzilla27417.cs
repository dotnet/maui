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
			var instructions = new Label { Text = @"There should be 6 buttons below. 
The first button should have the text 'Click Me' in the center.
The second button should have an image in the center and no text.
The third button should have the image on the left and the text on the right.
The fourth button should have the image on the top and the text on the bottom.
The fifth button should have the image on the right and the text on the left.
The sixth button should have the image on the bottom and the text on the top." };

			Content = new StackLayout
			{
				Spacing = 10,
				Children =
				{
					instructions,
					new ScrollView
					{
						Content = new StackLayout
						{
							Spacing = 10,
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.Center,
							Children =
							{
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