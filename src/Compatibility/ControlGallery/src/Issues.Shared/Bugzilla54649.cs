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
	[Issue(IssueTracker.Bugzilla, 54649, "[UWP] Navigation page title bar disappears upon navigating through tabs", PlatformAffected.UWP)]
	public class Bugzilla54649 : TestTabbedPage
	{
		protected override void Init()
		{
			Children.Add(CreateChildPage(1));
			Children.Add(CreateChildPage(2));
			Children.Add(CreateChildPage(3));
			Children.Add(CreateChildPage(4));
		}

		Page CreateChildPage(int number)
		{
			var content = new StackLayout
			{
				Children =
				{
					new Label { Text = $"This is page {number}" },
					new BoxView { Color = Colors.Red }
				}
			};

			var page = new ContentPage
			{
				Content = content,
				Title = $"Page {number}"
			};

			return new NavigationPage(page)
			{
				Title = $"Tab {number}"
			};
		}
	}
}