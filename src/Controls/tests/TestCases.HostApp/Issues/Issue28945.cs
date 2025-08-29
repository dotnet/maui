namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28945, "Add Focus propagation to MauiView", PlatformAffected.iOS)]
public class Issue28945 : TestContentPage
{
	Issue28945_ContentView _contentView;
	Label _statusLabel;

	const string FocusSuccessMessage = "ContentView Focused";
	const string UnfocusSuccessMessage = "ContentView UnFocused";

	protected override void Init()
	{
		Content = CreateContent();
		_contentView.Focused += FocusableContentView1_Focused;
		_contentView.Unfocused += FocusableContentView1_Unfocused;

	}

	VerticalStackLayout CreateContent()
	{
		var verticalStackLayout = new VerticalStackLayout
		{
			Spacing = 30,
			Children =
			{
				CreateContentView(),
				CreateLabel(),
			}
		};

		return verticalStackLayout;
	}

	Issue28945_ContentView CreateContentView()
	{
		_contentView = new Issue28945_ContentView
		{
			BackgroundColor = Colors.Green,
			Padding = new Thickness(30),
			Content = new Label
			{
				Text = "This is a focusable contentview, click the button to toggle focus",
				AutomationId = "Issue28945_ContentView",
				TextColor = Colors.White,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			}
		};

		return _contentView;
	}

	Label CreateLabel()
	{
		_statusLabel = new Label
		{
			AutomationId = "statusLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		return _statusLabel;
	}

	private void FocusableContentView1_Unfocused(object sender, FocusEventArgs e)
	{
		_statusLabel.Text = UnfocusSuccessMessage;
	}

	void FocusableContentView1_Focused(object sender, FocusEventArgs e)
	{
		_statusLabel.Text = FocusSuccessMessage;
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		if (_contentView.IsFocused)
		{
			_contentView.Unfocus();
		}
		else
		{
			_contentView.Focus();
		}
	}
}

public class Issue28945_ContentView : ContentView
{

}

#if IOS || MACCATALYST
public class Issue28945_ContentViewPlatform : Microsoft.Maui.Platform.ContentView
{
	public Issue28945_ContentViewPlatform()
	{
		UserInteractionEnabled = true;
	}

	public override bool CanBecomeFocused => true;
	public override bool CanBecomeFirstResponder => true;
}

public class Issue28945_ContentViewPlatformHandler : Microsoft.Maui.Handlers.ContentViewHandler
{
	public Issue28945_ContentViewPlatformHandler()
	{
	}

	protected override Microsoft.Maui.Platform.ContentView CreatePlatformView()
	{
		return new Issue28945_ContentViewPlatform();
	}

}
#endif

public static class Issue28945Extensions
{
	public static MauiAppBuilder Issue28945AddMappers(this MauiAppBuilder builder)
	{
		builder.ConfigureMauiHandlers(handlers =>
		{
#if IOS || MACCATALYST
			handlers.AddHandler<Issue28945_ContentView, Issue28945_ContentViewPlatformHandler>();
#endif
		});

		return builder;
	}
}