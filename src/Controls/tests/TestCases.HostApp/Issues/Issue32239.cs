namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32239, "RemovePage fails to disconnect handlers when the page is not visible", PlatformAffected.All)]
public class Issue32239 : Shell
{
	public Issue32239()
	{
		Items.Add(new ShellContent { Content = new Issue32239MainPage() });
	}
}

public class Issue32239MainPage : ContentPage
{
	public Issue32239MainPage()
	{
		Title = "Issue32239";

		var pushModalButton = new Button
		{
			Text = "Push Modal NavigationPage",
			AutomationId = "PushModalButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		pushModalButton.Clicked += async (s, e) =>
		{
			await Navigation.PushModalAsync(new NavigationPage(new Issue32239Page1()));
		};

		Content = new StackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20),
			Children = { pushModalButton }
		};
	}
}

public class Issue32239Page1 : ContentPage
{
	Issue32239Page2 _page2;

	public Issue32239Page1()
	{
		Title = "Page 1";

		var pushButton = new Button
		{
			Text = "Push Page 2",
			AutomationId = "PushPage2Button",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		pushButton.Clicked += async (s, e) =>
		{
			_page2 = new Issue32239Page2();
			await Navigation.PushAsync(_page2);
		};

		Content = new StackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20),
			Children = { pushButton }
		};
	}

	protected override void OnHandlerChanged()
	{
		base.OnHandlerChanged();

		if (Handler is null)
		{
			_page2?.UpdateStatus("Page 1 Handler Status: DISCONNECTED");
		}
	}
}
public class Issue32239Page2 : ContentPage
{
	Label _statusLabel;

	public Issue32239Page2()
	{
		Title = "Page 2";

		_statusLabel = new Label
		{
			Text = "Page 1 Handler Status: Waiting",
			AutomationId = "StatusLabel",
			FontSize = 16,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Start,
			Margin = new Thickness(20)
		};

		var removeButton = new Button
		{
			Text = "Remove Page 1 from Stack",
			AutomationId = "RemovePage1Button",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		removeButton.Clicked += (s, e) =>
		{
			var modalNav = Shell.Current?.Navigation?.ModalStack?.FirstOrDefault(p => p is NavigationPage) as NavigationPage;
			if (modalNav?.Navigation?.NavigationStack != null)
			{
				var pagesToRemove = modalNav.Navigation.NavigationStack.Take(modalNav.Navigation.NavigationStack.Count - 1).ToList();
				foreach (var page in pagesToRemove)
				{
					modalNav.Navigation.RemovePage(page);
				}
			}
		};

		Content = new StackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20),
			Children = { _statusLabel, removeButton }
		};
	}

	internal void UpdateStatus(string status)
	{
		_statusLabel.Text = status;
	}
}