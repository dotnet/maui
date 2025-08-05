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
		var button = new Button
		{
			Text = "Tap to Focus/Unfocus",
			AutomationId = "TglFocusButton",
		};

		button.Clicked += Button_Clicked;
		var verticalStackLayout = new VerticalStackLayout
		{
			Spacing = 30,
			Children =
			{
				CreateContentView(),
				CreateLabel(),
				button
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
				Text = "Tap Me",
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