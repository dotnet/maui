using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39395, "SwitchCell does not take all available place inside ListView",
		PlatformAffected.WinRT)]
	public class Bugzilla39395 : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				FontSize = 18,
				Text =
					"The switch cells below should be aligned with the right edge of the screen. If they are not, this test has failed."
			};

			Content = new StackLayout
			{
				BackgroundColor = Color.Gray,
				Children = {
					instructions,
					new ListView {
						ItemTemplate = new DataTemplate (typeof(SwitchCell)),
						ItemsSource = new[] { "Text", "Text" }
					}
				}
			};
		}
	}
}
