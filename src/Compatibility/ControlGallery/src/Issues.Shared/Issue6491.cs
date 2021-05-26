using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6491, "[Bug] Some Font Image are cropped on iOS",
		PlatformAffected.iOS)]
	public class Issue6491 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout
			{
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};

			var label = new Label
			{
				Text = "If you see the cloud in full, not clipped on the right, the test is passed"
			};

			var button = new Button
			{
				BackgroundColor = Colors.Red,
				WidthRequest = 200
			};

			button.ImageSource = new FontImageSource
			{
				Glyph = "\uf0c2",
				FontFamily = "FontAwesome5Free-Solid"
			};

			if (Device.RuntimePlatform == Device.UWP)
				((FontImageSource)button.ImageSource).FontFamily = "Assets/Fonts/fa-solid-900.ttf#Font Awesome 5 Free";

			stack.Children.Add(label);
			stack.Children.Add(button);

			Content = stack;
		}
	}
}