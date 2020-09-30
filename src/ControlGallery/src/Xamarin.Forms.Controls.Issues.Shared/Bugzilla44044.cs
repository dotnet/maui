using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44044, "TabbedPage steals swipe gestures", PlatformAffected.Android)]
	public class Bugzilla44044 : TestTabbedPage
	{
		string _btnToggleSwipe = "btnToggleSwipe";
		string _btnDisplayAlert = "btnDisplayAlert";

		protected override void Init()
		{
			Children.Add(new ContentPage()
			{
				Title = "Page 1",
				Content = new StackLayout
				{
					Children =
					{
						new Button
						{
							Text = "Click to Toggle Swipe Paging",
							Command = new Command(() => On<Android>().SetIsSwipePagingEnabled(!On<Android>().IsSwipePagingEnabled())),
							AutomationId = _btnToggleSwipe
						}
					}
				}
			});

			Children.Add(new ContentPage()
			{
				Title = "Page 2",
				Content = new StackLayout
				{
					Children =
					{
						new Button
						{
							Text = "Click to DisplayAlert",
							Command = new Command(() => DisplayAlert("Page 2", "Message", "Cancel")),
							AutomationId = _btnDisplayAlert
						}
					}
				}
			});
		}

#if UITEST && __ANDROID__
		[Test]
		public void Bugzilla44044Test()
		{
			RunningApp.WaitForElement(_btnToggleSwipe);
			
			RunningApp.SwipeRightToLeft();
			RunningApp.WaitForNoElement(_btnToggleSwipe);
			RunningApp.WaitForElement(_btnDisplayAlert);
			
			RunningApp.SwipeLeftToRight();
			RunningApp.WaitForNoElement(_btnDisplayAlert);
			RunningApp.WaitForElement(_btnToggleSwipe);
			
			RunningApp.Tap(_btnToggleSwipe);
			RunningApp.SwipeRightToLeft();
			RunningApp.WaitForNoElement(_btnDisplayAlert);
			RunningApp.WaitForElement(_btnToggleSwipe);
		}
#endif
	}
}