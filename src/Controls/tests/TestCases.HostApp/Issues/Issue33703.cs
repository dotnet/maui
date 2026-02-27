namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33703, "Empty space above TabBar after navigate back when TabBar visibility toggled", PlatformAffected.Android)]
public class Issue33703 : Shell
{
	public Issue33703()
	{
		var homeShellContent = new ShellContent
		{
			Content = new Issue33703HomePage()
		};
		Shell.SetFlyoutItemIsVisible(homeShellContent, false);

		var homeTab = new Tab { Title = "Home" };
		homeTab.Items.Add(homeShellContent);

		var secondTab = new Tab { Title = "Second Page", Route = "MyDevices" };
		secondTab.Items.Add(new ShellContent
		{
			Title = "Second Page",
			Content = new ContentPage
			{
				Title = "Second Page",
				Content = new Label
				{
					Text = "Second Page",
					AutomationId = "SecondTabLabel",
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center
				}
			}
		});

		var flyoutItem = new FlyoutItem
		{
			FlyoutDisplayOptions = FlyoutDisplayOptions.AsSingleItem,
			Route = "home"
		};
		Shell.SetFlyoutItemIsVisible(flyoutItem, false);
		flyoutItem.Items.Add(homeTab);
		flyoutItem.Items.Add(secondTab);

		Items.Add(flyoutItem);
	}
}

public class Issue33703HomePage : ContentPage
{
	public Issue33703HomePage()
	{
		Title = "Home";

		var stackLayout = new VerticalStackLayout
		{
			Spacing = 0,
			BackgroundColor = Colors.White
		};

		stackLayout.Add(new Label
		{
			Text = "Home",
			AutomationId = "InstructionLabel",
			FontSize = 13,
			Padding = 14,
			BackgroundColor = Colors.DarkOrange,
			TextColor = Colors.White
		});

		var btn = new Button
		{
			Text = "Go to Detail Page",
			AutomationId = "NavigateButton",
			Margin = new Thickness(20, 20, 20, 8)
		};
		btn.Clicked += async (s, e) => await Shell.Current.Navigation.PushAsync(new Issue33703DetailPage());
		stackLayout.Add(btn);

		for (int i = 1; i <= 15; i++)
		{
			stackLayout.Add(new Label
			{
				Text = $"Row {i}",
				Padding = new Thickness(20, 10),
				BackgroundColor = i % 2 == 0 ? Color.FromArgb("#EEEEEE") : Colors.White
			});
		}

		stackLayout.Add(new Label
		{
			Text = "Bottom",
			AutomationId = "BottomLabel",
			Padding = 14,
			BackgroundColor = Colors.LimeGreen,
			HorizontalOptions = LayoutOptions.Fill
		});

		Content = new ScrollView
		{
			Content = stackLayout,
			AutomationId = "MainScrollView"
		};
	}
}

public class Issue33703DetailPage : ContentPage
{
	public Issue33703DetailPage()
	{
		Title = "Detail Page";
		Shell.SetTabBarIsVisible(this, false);

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				new Label
				{
					Text = "Detail Page",
					AutomationId = "DetailLabel",
					FontSize = 26,
					FontAttributes = FontAttributes.Bold
				}
			}
		};
	}
}
