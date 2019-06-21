using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2577, "Hamburger icon not shown when using FormsAppCompatActivity", PlatformAffected.Android)]
	public class Issue2577 : TestMasterDetailPage
	{
		protected override void Init()
		{
			Master = new ContentPage { Title = "master page" };
			Detail = new NavigationPage(new DetailPage());
		}

		class DetailPage : ContentPage
		{
			public NavigationPage ParentPage => Parent as NavigationPage;

			public DetailPage()
			{
				var button = new Button { Text = "Click me" };
				button.Clicked += async (o, s) =>
				{
					var button2 = new Button { Text = "Toggle back button" };

					var page = new ContentPage { Content = new StackLayout { Children = {
							new Label { Text = "If there is no hamburger button, this test has failed. If you cannot toggle the back arrow, this test has failed." },
							button2
						} } };

					button2.Clicked += (o2, s2) =>
					{
						NavigationPage.SetHasBackButton(page, !NavigationPage.GetHasBackButton(page));
					};

					NavigationPage.SetHasBackButton(page, false);
					await ParentPage.PushAsync(page);
				};
				Content = button;
			}
		}
	}
}