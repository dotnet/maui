using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1026, "Label cropping", PlatformAffected.iOS | PlatformAffected.WinPhone, NavigationBehavior.PushModalAsync)]
	public class Issue1026 : ContentPage
	{
		public Issue1026()
		{
			var instructions = new Label
			{
				Text = "The label at the bottom of the form should read 'by subscribing," +
				" you accept the general conditions.'; if the label is truncated, this test has failed."
			};

			var scrollView =
				new ScrollView
				{
					BackgroundColor = Color.FromArgb("#dae1eb"),
					Content =
					new StackLayout
					{
						Padding = new Thickness(0, 18),
						Spacing = 10,
						Orientation = StackOrientation.Vertical,
						Children = {
						new Button {
							BackgroundColor = Color.FromArgb ("#006599"),
							TextColor = Colors.White,
							Text = "Subscribe with LinkedIn",
							WidthRequest = 262,
							HorizontalOptions = LayoutOptions.Center,
							CornerRadius = 0,
						},
						new Entry {
							Placeholder = "Professional email",
							WidthRequest = 262,
							HorizontalOptions = LayoutOptions.Center,
							Keyboard = Keyboard.Email,
						},
						new Entry {
							Placeholder = "Firstname",
							WidthRequest = 262,
							HorizontalOptions = LayoutOptions.Center,
						},
						new Entry {
							Placeholder = "Lastname",
							WidthRequest = 262,
							HorizontalOptions = LayoutOptions.Center,
						},
						new Entry {
							Placeholder = "Company",
							WidthRequest = 262,
							HorizontalOptions = LayoutOptions.Center,
						},
						new Entry {
							Placeholder = "Password",
							WidthRequest = 262,
							IsPassword = true,
							HorizontalOptions = LayoutOptions.Center,
						},
						new Entry {
							Placeholder = "Confirm password",
							WidthRequest = 262,
							IsPassword = true,
							HorizontalOptions = LayoutOptions.Center,
						},
						new Button {
							BackgroundColor = Color.FromArgb ("#05addc"),
							TextColor = Colors.White,
							Text = "Create an account",
							WidthRequest = 262,
							HorizontalOptions = LayoutOptions.Center,
							CornerRadius = 0,
						},
						new Label {
							Text = "by subscribing, you accept the general conditions.",
							TextColor = Colors.White,
							HorizontalTextAlignment = TextAlignment.Center,
							FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
							WidthRequest = 262,
							HorizontalOptions = LayoutOptions.Center,
						},
					},
					},

				};

			Content = new StackLayout { Children = { instructions, scrollView } };
		}
	}
}
