using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.PlatformConfiguration;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44044, "TabbedPage steals swipe gestures", PlatformAffected.Android)]
	public class Bugzilla44044 : TestTabbedPage
	{
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
							Command = new Command(() => On<Android>().SetIsSwipePagingEnabled(!On<Android>().IsSwipePagingEnabled()))
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
							Command = new Command(() => DisplayAlert("Page 2", "Message", "Cancel"))
						}
					}
				}
			});
		}
	}
}