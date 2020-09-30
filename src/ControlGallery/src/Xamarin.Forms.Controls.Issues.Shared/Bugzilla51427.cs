using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51427, "Custom Font Not Working Inside NavigationPage", PlatformAffected.UWP)]
	public class Bugzilla51427 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new TestFontPage());
		}
	}

	class TestFontPage : ContentPage
	{
		public TestFontPage()
		{
			var label = new Label
			{
				Text = "This Label should be using a custom font.",
				VerticalOptions = LayoutOptions.CenterAndExpand,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				FontSize = 24,
				FontFamily = "Assets/Fonts/Lobster-Regular.ttf#Lobster"
			};

			Content = label;
		}
	}
}