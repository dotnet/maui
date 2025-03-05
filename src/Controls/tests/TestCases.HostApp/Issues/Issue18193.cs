namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18193, "[iOS] Navigation doesn't work on sixth tab in shell", PlatformAffected.iOS)]
	public class Issue18193 : Shell
	{
		public Issue18193()
		{
			var tabBar = new TabBar();

			tabBar.Items.Add(CreateShellContent("MainPage", typeof(Issue18193MainPage), nameof(Issue18193MainPage)));
			tabBar.Items.Add(CreateShellContent("Page 2", typeof(Issue18193Page2), nameof(Issue18193Page2)));
			tabBar.Items.Add(CreateShellContent("Page 3", typeof(Issue18193Page3), nameof(Issue18193Page3)));
			tabBar.Items.Add(CreateShellContent("Page 4", typeof(Issue18193Page4), nameof(Issue18193Page4)));
			tabBar.Items.Add(CreateShellContent("Page 5", typeof(Issue18193Page5), nameof(Issue18193Page5)));
			tabBar.Items.Add(CreateShellContent("Page 6", typeof(Issue18193Page6), nameof(Issue18193Page6)));
			//Added the TabBar Items ,this will show overflow menu in Windows
			//to access the extra pages, keeping the interface clean and easy to navigate.
			tabBar.Items.Add(CreateShellContent("Page 7", typeof(Issue18193Page7), nameof(Issue18193Page7)));
			tabBar.Items.Add(CreateShellContent("Page 8", typeof(Issue18193Page8), nameof(Issue18193Page8)));
			tabBar.Items.Add(CreateShellContent("Page 9", typeof(Issue18193Page9), nameof(Issue18193Page9)));
			tabBar.Items.Add(CreateShellContent("Page 10", typeof(Issue18193Page10), nameof(Issue18193Page10)));
			tabBar.Items.Add(CreateShellContent("Page 11", typeof(Issue18193Page11), nameof(Issue18193Page11)));
			tabBar.Items.Add(CreateShellContent("Page 12", typeof(Issue18193Page12), nameof(Issue18193Page12)));
			tabBar.Items.Add(CreateShellContent("Page 13", typeof(Issue18193Page13), nameof(Issue18193Page13)));
			Items.Add(tabBar);
			Routing.RegisterRoute(nameof(Issue18193DetailPage), typeof(Issue18193DetailPage));
		}

		ShellContent CreateShellContent(string title, Type contentType, string route) => new()
		{
			Title = title,
			ContentTemplate = new DataTemplate(contentType),
			Route = route
		};
	}

	class Issue18193MainPage : ContentPage
	{
		public Issue18193MainPage()
		{
			Button sixthPageButton = new Button() { AutomationId = "NavigationToPageSixthButton", Text = "Navigate to page 6" };
			sixthPageButton.Clicked += (s, e) => Issue18193.Current.GoToAsync("//" + nameof(Issue18193Page6));
			Content = sixthPageButton;
		}
	}

	class Issue18193DetailPage : ContentPage
	{
		public Issue18193DetailPage()
		{
			Title = "Detail Page";
			Button backButton = new Button() { AutomationId = "NavigateBackButton", Text = "Navigate back" };
			backButton.Clicked += (s, e) => Issue18193.Current.GoToAsync("..");
			Content = backButton;
		}
	}

	class Issue18193Page2 : ContentPage
	{
		public Issue18193Page2()
		{
			Content = new Button()
			{
				Text = "Navigate to page 5",
				Command = new Command(async () => await Issue18193.Current.GoToAsync("//" + nameof(Issue18193Page5))),
				AutomationId = "NavigateToPageFiveButton"
			};
		}
	}

	public class Issue18193Page3 : ContentPage { }

	public class Issue18193Page4 : ContentPage { }

	public class Issue18193Page5 : ContentPage { }
	public class Issue18193Page7 : ContentPage { }

	public class Issue18193Page8 : ContentPage { }

	public class Issue18193Page9 : ContentPage { }

	public class Issue18193Page10 : ContentPage { }

	public class Issue18193Page11 : ContentPage { }

	public class Issue18193Page12 : ContentPage { }

	public class Issue18193Page13 : ContentPage { }

	public class Issue18193Page6 : ContentPage
	{
		public Issue18193Page6()
		{
			Button detailPageButton = new Button() { AutomationId = "NavigateToDetailButton", Text = "Navigate to detail page" };
			detailPageButton.Clicked += (s, e) => Issue18193.Current.GoToAsync(nameof(Issue18193DetailPage));

			Button secondPageButton = new Button() { AutomationId = "NavigateToPageTwoButton", Text = "Navigate to Page 2" };
			secondPageButton.Clicked += (s, e) => Issue18193.Current.GoToAsync("//" + nameof(Issue18193Page2));
			Content = new StackLayout
			{
				Children =
				{
					detailPageButton,
					secondPageButton
				}
			};
		}
	}
}