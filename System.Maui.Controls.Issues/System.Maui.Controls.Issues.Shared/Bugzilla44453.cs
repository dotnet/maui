using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

// Apply the default category of "Issues" to all of the tests in this assembly
// We use this as a catch-all for tests which haven't been individually categorized
#if UITEST
[assembly: NUnit.Framework.Category("Issues")]
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44453, "[UWP] ToolbarItem Text hard to see when BarTextColor is light", PlatformAffected.WinRT)]
	public class Bugzilla44453 : TestMasterDetailPage
	{
		protected override void Init()
		{
			var content = new ContentPage
			{
				Title = "UWPToolbarItemColor",
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children =
					{
						new Label
						{
							LineBreakMode = LineBreakMode.WordWrap,
							HorizontalTextAlignment = TextAlignment.Center,
							Text = "The toolbar secondary items should not have white text on a light background"
						}
					}
				}
			};
			
			MasterBehavior = MasterBehavior.Popover;
			Master = new ContentPage
			{
				Title = "Master"
			};
			Detail = new NavigationPage(content)
			{
				BarBackgroundColor = Color.Green,
				BarTextColor = Color.White
			};

			Detail.ToolbarItems.Add(new ToolbarItem("Test Secondary Item", null, delegate { }, ToolbarItemOrder.Secondary));
		}
	}
}