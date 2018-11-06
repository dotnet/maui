using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "[UWP] Modal NavigationPage ignores BarTextColor settings for icons", PlatformAffected.WinRT)]
	public class AppBarIconColors : TestNavigationPage
	{
		protected override void Init()
		{
			var firstPage = new ContentPage() { Title = "Page One" };

			var button = new Button { Text = "Push Modal Page" };

			firstPage.Content = new StackLayout { Children = { new Label { Text = "Click the 'push modal page' button" }, button } };

			var otherButton = new Button() { Text = "back" };
			otherButton.Clicked += (sender, args) => Navigation.PopModalAsync();

			var page = new ContentPage {
				Title = "Page Two",
				Content =
					new StackLayout {
						Children = {
							new Label {
								Text =
									"This is a modal page. The 'X' icon, the 'Done' label below it, and the '...' in the toolbar should all be white on UWP. If any of them are not white, this test has failed."
							},
							otherButton
						}
					}
			};
			page.ToolbarItems.Add(new ToolbarItem("Done", "toolbar_close.png", () => { Navigation.PopModalAsync(); }));

			button.Clicked += (sender, args) => Navigation.PushModalAsync(new NavigationPageWithAppBarColors(page));

			PushAsync(new NavigationPageWithAppBarColors(firstPage));
		}
	}

	[Preserve(AllMembers = true)]
	public class NavigationPageWithAppBarColors : NavigationPage
	{
		public NavigationPageWithAppBarColors(Page root) : base(root)
		{
			BarBackgroundColor = Color.Purple;
			BarTextColor = Color.White;
			Title = root.Title;
			Icon = root.Icon;
		}
	}
}
