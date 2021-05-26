using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 55714, "[UWP] Cannot set Editor text color", PlatformAffected.UWP)]
	public class Bugzilla55714 : TestContentPage
	{
		protected override void Init()
		{
			var editor = new Editor
			{
				TextColor = Colors.Yellow,
				BackgroundColor = Colors.Black
			};

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "The below Editor should have visible yellow text when entered"
					},
					editor,
					new Button
					{
						Text = "Change Editor text color to white",
						Command = new Command(() => editor.TextColor = Colors.White)
					}
				}
			};
		}
	}
}