using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 38105, "RemovePage does not cause back arrow to go away on Android", PlatformAffected.Android, navigationBehavior: NavigationBehavior.PushModalAsync)]
	public class Bugzilla38105 : TestMasterDetailPage
	{
		protected override void Init ()
		{
			Detail = new NavigationPage (new ViewA38105 ());

			var button = new Button () { Text = "Click me" };
			button.Clicked += (o, e) => {
				var navPage = (NavigationPage)Detail;

				var rootPage = navPage.CurrentPage;

				navPage.PopToRootAsync (false);

				navPage.Navigation.PushAsync (new ViewB38105 ());

				navPage.Navigation.RemovePage (rootPage);

				IsPresented = false;
			};

			Master = new ContentPage () {
				Title = "test",
				Content = button
			};
		}

		[Preserve(AllMembers = true)]
		class ViewA38105 : ContentPage
		{
			public ViewA38105 ()
			{
				Title = "View A";

				Content = new Label
				{
					Text = "Verify that the page title is currently \"View A\". Open master detail menu and click the content button. " +
					"Verify that the page title is now \"View B\" and that the hamburger icon is NOT replaced by the back arrow.",
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center,
					LineBreakMode = LineBreakMode.WordWrap,
					MaxLines = 5
				};
			}
		}

		[Preserve(AllMembers = true)]
		class ViewB38105 : ContentPage
		{
			public ViewB38105 ()
			{
				Title = "View B";
			}
		}
	}
}
