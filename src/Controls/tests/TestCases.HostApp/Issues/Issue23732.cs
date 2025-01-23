using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23732, "TabBar content not displayed properly", PlatformAffected.Android)]
	public partial class Issue23732 : TabbedPage
	{
		public Issue23732()
		{
			var page1 = CreateNavigationPage("Page 1", "page1", "1");
			var page2 = CreateNavigationPage("Page 2", "page2", "2");
			var page3 = CreateNavigationPage("Page 3", "page3", "3");
			var page4 = CreateNavigationPage("Page 4", "page4", "4");
			var page5 = CreateNavigationPage("Page 5", "page5", "5");

			Children.Add(page1);
			Children.Add(page2);
			Children.Add(page3);
			Children.Add(page4);
			Children.Add(page5);
		}

		private NavigationPage CreateNavigationPage(string title, string label, string icon)
		{
			var mainPage = new _23732Page
			{
				Label = label,
				Icon = icon
			};

			return new NavigationPage(mainPage)
			{
				Title = title
			};
		}
	}
}