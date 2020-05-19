using System.Maui.CustomAttributes;
using System.Maui.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43519, "[UWP] MasterDetail page ArguementException when nested in a TabbedPage and returning from modal page"
		, PlatformAffected.UWP)]

#if UITEST
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	public class Bugzilla43519 : TestTabbedPage
	{
		const string Pop = "PopModal";

		const string Push = "PushModal";

		const string Page2 = "Page 2";

		protected override void Init()
		{
			var modalPage = new ContentPage
			{
				Title = "ModalPage",
				Content = new StackLayout
				{
					Children =
					{
						new Button
						{
							Command = new Command(() => Navigation.PopModalAsync()),
							Text = "Pop modal page -- should not crash on UWP",
							AutomationId = Pop
						}
					}
				}
			};

			var mdp = new MasterDetailPage
			{
				Title = "Page 1",
				Master = new ContentPage
				{
					Title = "Master",
					Content = new StackLayout()
				},
				Detail = new ContentPage
				{
					Title = "Detail",
					Content = new StackLayout()
				}
			};

			Children.Add(mdp);
			Children.Add(new ContentPage
			{
				Title = Page2,
				Content = new StackLayout
				{
					Children =
					{
						new Button
						{
							Command = new Command(() => Navigation.PushModalAsync(modalPage)),
							Text = "Click to display modal",
							AutomationId = Push
						}
					}
				}
			});
		}

#if UITEST && __WINDOWS__

		[Test]
		public void TabbedModalNavigation()
		{
			RunningApp.WaitForElement(Page2);
			RunningApp.Tap(Page2);
			RunningApp.WaitForElement(Push);
			RunningApp.Tap(Push);
			RunningApp.WaitForElement(Pop);
			RunningApp.Tap(Pop);
			RunningApp.WaitForElement(Page2);
		}
#endif
	}
}