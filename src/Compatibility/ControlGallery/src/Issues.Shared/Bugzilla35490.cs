using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 35490, "Label Text Misaligned in Windows Phone 8.1 and WinRT",
		PlatformAffected.WinPhone | PlatformAffected.WinRT)]
	public class Bugzilla35490 : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				Text =
					"The label in the red box below should be centered horizontally and vertically. If it's not, this test has failed."
			};


			var label = new Label
			{
				BackgroundColor = Colors.Red,
				TextColor = Colors.White,
				HorizontalTextAlignment = TextAlignment.Center,
				VerticalTextAlignment = TextAlignment.Center,
				HeightRequest = 200,
				HorizontalOptions = LayoutOptions.Fill,
				Text = "Should be centered horizontally and vertically"
			};


			Content = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
				Children = {
					instructions,
					label
				}
			};
		}
	}
}
