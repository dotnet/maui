using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 27731, "[Android] Action Bar can not be controlled reliably on MasterDetailPage", PlatformAffected.Android)]
	public class Bugzilla27731 : TestMasterDetailPage
	{
		string _pageTitle = "PageTitle";
		protected override void Init()
		{
			// Initialize ui here instead of ctor
			Master = new ContentPage { Content = new Label { Text = "Menu Item" }, Title = "Menu" };
			Detail = new NavigationPage(new Page2(_pageTitle));
		}

		class Page2 : ContentPage
		{
			static int count;
			public Page2(string title)
			{
				count++;
				Title = $"{title}{count}";
				NavigationPage.SetHasNavigationBar(this, false);
				Content = new StackLayout
				{
					Children =
				{
					new Label { Text = $"This is page {count}." },
					new Button { Text = "Click", Command = new Command(() => Navigation.PushAsync(new Page2(title))) }
				}
				};
			}
		}

#if UITEST
		[Test]
		public void Bugzilla27731Test()
		{
			RunningApp.WaitForNoElement(_pageTitle);
		}
#endif
	}
}