namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38949, "Updating Window.TitleBar while pushing a modal page crashes on Windows", PlatformAffected.UWP)]
public class Issue38949 : NavigationPage
{
    public Issue38949() : base(new Issue38949ContentPage())
    {
    }
}

class Issue38949ContentPage : ContentPage
{
	readonly string _labelStyleKey = typeof(Label).FullName!;
	readonly bool _hadLabelStyle;
	readonly object _originalLabelStyle;
	readonly TitleBar _titleBar;
	bool _isPushingModal;
	bool _resourcesRestored;

	public Issue38949ContentPage()
	{
		var resources = Application.Current!.Resources;

		_hadLabelStyle = resources.TryGetValue(_labelStyleKey, out var originalLabelStyle);
		_originalLabelStyle = originalLabelStyle;

		var labelStyle = new Style(typeof(Label));
		labelStyle.Setters.Add(new Setter
		{
			Property = Label.BackgroundColorProperty,
			Value = Colors.Transparent
		});

		resources[_labelStyleKey] = labelStyle;

		_titleBar = new TitleBar
		{
			Title = "Test Title",
			Subtitle = "Test Subtitle"
		};

		Button pushModalButton = new Button
		{
			Text = "Push Modal Page",
			AutomationId = "PushModalPageButton"
		};

		pushModalButton.Clicked += async (_, _) =>
		{
			_isPushingModal = true;
			try
			{
				await Navigation.PushModalAsync(new Issue38949ModalPage());
			}
			finally
			{
				_isPushingModal = false;
			}
		};

		Content = new VerticalStackLayout
		{
			Spacing = 12,
			Padding = new Thickness(12),
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "TitleBar modal regression test",
					AutomationId = "Issue38949PageLabel",
					HorizontalOptions = LayoutOptions.Center
				},
				pushModalButton
			}
		};
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		base.OnNavigatedTo(args);

		if (Window is not null)
		{
			Window.TitleBar = _titleBar;
		}
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		if (!_isPushingModal)
		{
			RestoreApplicationResources();
		}
	}

	void RestoreApplicationResources()
	{
		if (_resourcesRestored)
		{
			return;
		}

		_resourcesRestored = true;

		var resources = Application.Current!.Resources;
		if (_hadLabelStyle)
		{
			resources[_labelStyleKey] = _originalLabelStyle!;
		}
		else
		{
			resources.Remove(_labelStyleKey);
		}
	}
}

class Issue38949ModalPage : ContentPage
{
    public Issue38949ModalPage()
    {
        Button popModalButton = new Button
        {
            Text = "Pop Modal Page",
            AutomationId = "PopModalPageButton",
            Command = new Command(async () => await Navigation.PopModalAsync(false))
        };

		Content = new VerticalStackLayout
		{
			Spacing = 12,
			Padding = new Thickness(12),
			VerticalOptions = LayoutOptions.Center,
			Children =
			{
				new Label
				{
					Text = "Modal page displayed",
					AutomationId = "ModalPageLabel",
					HorizontalOptions = LayoutOptions.Center
				},
				popModalButton
			}
		};
	}
}