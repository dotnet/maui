namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34343, "TabBar displays wrong tabs after first tab becomes invisible", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue34343 : Shell
{
	ContentPage _tab1Page;

	public Issue34343()
	{
		// Register Page51 as a sub-route for navigating from Tab5 (iOS repro)
		Routing.RegisterRoute("Page51", typeof(Issue34343_Page51));

		var tabBar = new TabBar();

		// Tab 1 - first tab that will be hidden to trigger the bug
		var button = new Button
		{
			Text = "Hide Tab1 and Go to Tab5",
			AutomationId = "HideAndNavigateButton",
		};

		_tab1Page = new ContentPage
		{
			Title = "Tab1",
			Content = new VerticalStackLayout
			{
				Children = { button }
			}
		};

		button.Clicked += async (s, e) =>
		{
			// Setting IsVisible = false on the page (as in the issue repro: "this.IsVisible = false")
			_tab1Page.IsVisible = false;
			await Shell.Current.GoToAsync("///Tab5");
		};

		var tab1Content = new ShellContent
		{
			Title = "Tab1",
			Route = "Tab1",
			Content = _tab1Page
		};

		var tab1 = new Tab { Title = "Tab1", AutomationId = "Tab1" };
		tab1.Items.Add(tab1Content);
		tabBar.Items.Add(tab1);

		// Tabs 2-4: use ContentTemplate (lazy creation) to reproduce the Android bug.
		// With ContentTemplate, the page is null until first shown, so DisplayedPage=null
		// when SetupMenu fires after Tab1 is hidden — causing SetupMenu to return early
		// and the tab bar to remain unrebuilt (the core Android bug trigger).
		for (int i = 2; i <= 4; i++)
		{
			int tabNum = i; // capture for closure
			var content = new ShellContent
			{
				Title = $"Tab{tabNum}",
				Route = $"Tab{tabNum}",
				ContentTemplate = new DataTemplate(() => new ContentPage
				{
					Title = $"Tab{tabNum}",
					Content = new Label
					{
						Text = $"Tab{tabNum} Content",
						AutomationId = $"Tab{tabNum}Content",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				})
			};
			var tab = new Tab { Title = $"Tab{tabNum}", AutomationId = $"Tab{tabNum}" };
			tab.Items.Add(content);
			tabBar.Items.Add(tab);
		}

		// Tab 5 - has a button to navigate to Page51 (iOS repro: relative sub-page navigation)
		var navigateToPage51Button = new Button
		{
			Text = "Navigate to Page51",
			AutomationId = "NavigateToPage51Button",
		};
		navigateToPage51Button.Clicked += async (s, e) =>
		{
			await Shell.Current.GoToAsync("Page51");
		};

		var tab5Page = new ContentPage
		{
			Title = "Tab5",
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Tab5 Content",
						AutomationId = "Tab5Content",
						HorizontalOptions = LayoutOptions.Center
					},
					navigateToPage51Button
				}
			}
		};

		var tab5Content = new ShellContent { Title = "Tab5", Route = "Tab5", Content = tab5Page };
		var tab5 = new Tab { Title = "Tab5", AutomationId = "Tab5" };
		tab5.Items.Add(tab5Content);
		tabBar.Items.Add(tab5);

		// Tabs 6-7: use ContentTemplate (lazy creation) for the same reason as Tabs 2-4
		for (int i = 6; i <= 7; i++)
		{
			int tabNum = i; // capture for closure
			var content = new ShellContent
			{
				Title = $"Tab{tabNum}",
				Route = $"Tab{tabNum}",
				ContentTemplate = new DataTemplate(() => new ContentPage
				{
					Title = $"Tab{tabNum}",
					Content = new Label
					{
						Text = $"Tab{tabNum} Content",
						AutomationId = $"Tab{tabNum}Content",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				})
			};
			var tab = new Tab { Title = $"Tab{tabNum}", AutomationId = $"Tab{tabNum}" };
			tab.Items.Add(content);
			tabBar.Items.Add(tab);
		}

		Items.Add(tabBar);
	}
}

// Sub-page navigated to from Tab5 (relative route "Page51") — iOS repro
public class Issue34343_Page51 : ContentPage
{
	public Issue34343_Page51()
	{
		Title = "Page51";
		Content = new Label
		{
			Text = "Page51 Content",
			AutomationId = "Page51Content",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
	}
}


