using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43519, "[UWP] MasterDetail page ArguementException when nested in a TabbedPage and returning from modal page", PlatformAffected.UWP)]
	public class Bugzilla43519 : TestTabbedPage
	{
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
							Text = "Pop modal page -- should not crash on UWP"
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
				Title = "Page 2",
				Content = new StackLayout
				{
					Children =
					{
						new Button
						{
							Command = new Command(() => Navigation.PushModalAsync(modalPage)),
							Text = "Click to display modal"
						}
					}
				}
			});
		}
	}
}