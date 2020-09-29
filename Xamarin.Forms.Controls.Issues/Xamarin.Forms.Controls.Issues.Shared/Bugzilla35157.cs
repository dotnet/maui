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
	[Issue(IssueTracker.Bugzilla, 35157, "CarouselPage inside NavPage inside TabbedPage gets laid out incorrectly", NavigationBehavior.PushModalAsync)]
	public class Bugzilla35157 : TestTabbedPage
	{
		protected override void Init()
		{
			var button = new Button
			{
				Text = "Go",
				AutomationId = "firstButton"
			};


			button.Clicked += (sender, args) =>
			{
				Button button2 = null;
				button.Navigation.PushAsync(new CarouselPage
				{
					Children = {
						new ContentPage {
							Content = button2 = new Button {
								AutomationId = "secondButton",
								VerticalOptions = LayoutOptions.EndAndExpand,
								Text = "Click Me"
							}
						}
					}
				});

				button2.Clicked += (s, a) => button2.Text = "Button Clicked!";
			};

			var tab = new NavigationPage(new ContentPage { Content = button });
			tab.Title = "Tab";
			Children.Add(tab);


		}

#if UITEST
		[Test]
		public void ButtonCanBeClicked ()
		{
			RunningApp.WaitForElement (q => q.Marked ("firstButton"));
			RunningApp.Tap (q => q.Marked ("firstButton"));
			RunningApp.WaitForElement (q => q.Marked ("secondButton"));
			RunningApp.Tap (q => q.Marked ("secondButton"));
			RunningApp.WaitForElement (q => q.Button ("Button Clicked!"));
		}
#endif
	}
}