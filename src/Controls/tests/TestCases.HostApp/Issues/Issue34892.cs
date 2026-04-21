namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34892, "ContentPage with ToolbarItem Clicked event leaks when presented as modal page", PlatformAffected.iOS)]
public class Issue34892 : Shell
{
	public Issue34892()
	{
		Items.Add(new ShellContent
		{
			ContentTemplate = new DataTemplate(() => new Issue34892MainPage())
		});
	}
}

public class Issue34892MainPage : ContentPage
{
	readonly List<WeakReference> _pageRefs = new List<WeakReference>();
	readonly Label _statusLabel;

	public Issue34892MainPage()
	{
		Title = "ToolbarItem Leak Repro";

		var pushLeakPageModalButton = new Button
		{
			Text = "Push LeakPage (Modal)",
			AutomationId = "PushLeakPageModalButton"
		};
		pushLeakPageModalButton.Clicked += OnPushLeakPageModal;

		var forceGcButton = new Button
		{
			Text = "Force GC & Check",
			AutomationId = "ForceGCButton"
		};
		forceGcButton.Clicked += OnForceGC;

		_statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			FontSize = 14,
			TextColor = Colors.Red
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(30),
			Spacing = 20,
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				pushLeakPageModalButton,
				forceGcButton,
				_statusLabel
			}
		};
	}

	async void OnPushLeakPageModal(object sender, EventArgs e)
	{
		var page = new Issue34892LeakPage();
		_pageRefs.Add(new WeakReference(page));
		await Navigation.PushModalAsync(new NavigationPage(page));
	}

	async void OnForceGC(object sender, EventArgs e)
	{
		try
		{
			await GarbageCollectionHelper.WaitForGC(2000, _pageRefs.ToArray());
		}
		catch { }

		var alive = _pageRefs.Count(wr => wr.IsAlive);
		_statusLabel.Text = $"Still alive: {alive}";
	}
}

public class Issue34892LeakPage : ContentPage
{
	static int _instanceCount;
	readonly int _id;

	public Issue34892LeakPage()
	{
		Title = "Leak Page";
		_id = ++_instanceCount;

		var toolbarItem = new ToolbarItem
		{
			Text = "Action"
		};
		toolbarItem.Clicked += OnToolbarItemClicked;
		ToolbarItems.Add(toolbarItem);

		var closeButton = new Button
		{
			Text = "Close",
			AutomationId = "LeakPageCloseButton"
		};
		closeButton.Clicked += OnCloseClicked;

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children =
			{
				new Label
				{
					Text = "This page has a ToolbarItem with a Clicked event.",
					HorizontalOptions = LayoutOptions.Center
				},
				new Label
				{
					Text = "Dismiss this page and check if it gets collected.",
					HorizontalOptions = LayoutOptions.Center
				},
				closeButton
			}
		};
	}

	void OnToolbarItemClicked(object sender, EventArgs e)
	{
	}

	async void OnCloseClicked(object sender, EventArgs e)
	{
		if (Navigation.ModalStack.Count > 0)
		{
			await Navigation.PopModalAsync();
		}
		else
		{
			await Navigation.PopAsync();
		}
	}

}
