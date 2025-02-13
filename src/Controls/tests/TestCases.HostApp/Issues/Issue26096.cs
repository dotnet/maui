namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 26069, "The TabbedPage selection indicator is not updated properly when reloading the TabbedPage with a new instance",
			PlatformAffected.UWP)]
	public class Issue26069 : TestContentPage
	{
		protected override void Init()
		{
			Button tabbedPageButton = new Button();
			tabbedPageButton.AutomationId = "OpenTabbedPage";
			tabbedPageButton.HorizontalOptions = LayoutOptions.Center;
			tabbedPageButton.VerticalOptions = LayoutOptions.Center;
			tabbedPageButton.Text = "Open Tabbed Page";
			tabbedPageButton.Clicked += TabbedPageButton_Clicked;

			VerticalStackLayout layout = new VerticalStackLayout();
			layout.VerticalOptions = LayoutOptions.Center;
			layout.Children.Add(tabbedPageButton);
			Content = layout;
		}

		private async void TabbedPageButton_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new NavigationPage(new Issue26069TabbedPage()));
		}
	}

	public class Issue26069TabbedPage : TabbedPage
	{
		public Issue26069TabbedPage()
		{
			ContentPage page1 = new ContentPage();
			page1.Title = "Page 1";

			ContentPage page2 = new ContentPage();
			page2.Title = "Page 2";

			ContentPage page3 = new ContentPage();
			page3.Title = "Page 3";

			this.Children.Add(page1);
			this.Children.Add(page2);
			this.Children.Add(page3);
			ToolbarItems.Add(new ToolbarItem("Back", null, async () =>
			{
				await Navigation.PopModalAsync(true);
			}));
		}
	}
}
