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
			tabBar.Items.Add(CreateShellContent("Page 7", typeof(Issue18193Page2), nameof(Issue18193Page7)));
			tabBar.Items.Add(CreateShellContent("Page 8", typeof(Issue18193Page3), nameof(Issue18193Page8)));
			tabBar.Items.Add(CreateShellContent("Page 9", typeof(Issue18193Page4), nameof(Issue18193Page9)));
			tabBar.Items.Add(CreateShellContent("Page 10", typeof(Issue18193Page5), nameof(Issue18193Page10)));
			tabBar.Items.Add(CreateShellContent("Page 11", typeof(Issue18193Page6), nameof(Issue18193Page11)));
			tabBar.Items.Add(CreateShellContent("Page 12", typeof(Issue18193Page2), nameof(Issue18193Page12)));
			tabBar.Items.Add(CreateShellContent("Page 13", typeof(Issue18193Page3), nameof(Issue18193Page13)));
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
			var button = new Button() { AutomationId = "NavigationToPage6Button", Text = "Navigate to page 6" };
			button.Clicked += (s, e) => Issue18193.Current.GoToAsync("//" + nameof(Issue18193Page6));
			Content = button;
		}
	}

	class Issue18193DetailPage : ContentPage
	{
		public Issue18193DetailPage()
		{
			Title = "Detail Page";
			var button = new Button() { AutomationId = "NavigateBackButton", Text = "Navigate back" };
			button.Clicked += (s, e) => Issue18193.Current.GoToAsync("..");
			Content = button;
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
				AutomationId = "NavigateToPage5Button"
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
			var button = new Button() { AutomationId = "NavigateToDetailButton", Text = "Navigate to detail page" };
			button.Clicked += (s, e) => Issue18193.Current.GoToAsync(nameof(Issue18193DetailPage));

			var button2 = new Button() { AutomationId = "NavigateToPage2Button", Text = "Navigate to Page 2" };
			button2.Clicked += (s, e) => Issue18193.Current.GoToAsync("//" + nameof(Issue18193Page2));
			Content = new StackLayout
			{
				Children =
				{
					button,
					button2
				}
			};
		}
	}
}