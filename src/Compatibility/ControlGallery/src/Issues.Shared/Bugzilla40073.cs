using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40073, "Toolbar items are not not functioning properly on UWP", PlatformAffected.WinRT)]
	public class Bugzilla40073 : TestNavigationPage
	{
		ContentPage _theContent;

		protected override void Init()
		{
			_theContent = new ContentPage
			{
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
						new Label {
							HorizontalTextAlignment = TextAlignment.Center,
							Text = "This page should have a toolbar. If it does not, the test has failed."
						}
					}
				}
			};

			var thePage = new TabbedPage();
			thePage.Children.Add(_theContent);
			thePage.ToolbarItems.Add(new ToolbarItem() { Text = "Refresh", IconImageSource = "coffee.png" });

			PushAsync(thePage);
		}
	}
}