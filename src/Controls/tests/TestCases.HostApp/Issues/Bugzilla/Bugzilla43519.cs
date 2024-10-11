using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43519, "[UWP] FlyoutPage ArgumentException when nested in a TabbedPage and returning from modal page"
		, PlatformAffected.UWP)]
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

			var mdp = new FlyoutPage
			{
				Title = "Page 1",
				Flyout = new ContentPage
				{
					Title = "Flyout",
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
				AutomationId = Page2,
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
	}
}