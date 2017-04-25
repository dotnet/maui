using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 51173, "Custom ImageRenderer error handling demo", PlatformAffected.All)]
	public class CustomImageRendererErrorHandling : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout { Margin = new Thickness(5, 40, 5, 0) };

			var instructions = new Label
			{
				Text =
					@"
Click 'Update Image Source'; it will update the coffee image with an image source which will throw an exception. 
Instead of just logging an error, the custom renderer will display an alert dialog about the error.	
"
			};

			var image = new _51173Image { Source = "coffee.png" };

			var button = new Button { Text = "Update Image Source" };

			button.Clicked += (sender, args) => image.Source = new FailImageSource();

			layout.Children.Add(instructions);
			layout.Children.Add(image);
			layout.Children.Add(button);

			Content = layout;
		}
	}

	// custom image type for demonstrating custom error handling in a custom renderer
	[Preserve(AllMembers = true)]
	public class _51173Image : Image { }
}