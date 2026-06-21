using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33067, "[Windows, Android] ScrollView Content Not Removed When Set to Null", PlatformAffected.Android | PlatformAffected.UWP)]

public class Issue33067 : ContentPage
{
	ScrollView _scrollView = null!;
	Label _originalContent = null!;
	Button _setNullButton = null!;
	Button _addContentButton = null!;

	public Issue33067()
	{
		CreateUI();
	}

	void CreateUI()
	{
		// Create the original content label
		_originalContent = new Label
		{
			Text = "This is a sample label inside the ScrollView that can be set to null and added back.",
			Padding = new Thickness(20),
			AutomationId = "ContentLabel",
			FontSize = 16
		};

		// Create the ScrollView
		_scrollView = new ScrollView
		{
			BackgroundColor = Colors.LightGray,
			HeightRequest = 300,
			Content = null
		};

		// Create the "Set Content to Null" button
		_setNullButton = new Button
		{
			Text = "Set Content to Null",
			AutomationId = "SetNullButton"
		};
		_setNullButton.Clicked += OnSetContentNullClicked;

		// Create the "Add Content" button
		_addContentButton = new Button
		{
			Text = "Add Content",
			AutomationId = "AddContentButton"
		};
		_addContentButton.Clicked += OnAddContentClicked;

		// Create the main layout
		var layout = new VerticalStackLayout
		{
			Spacing = 20,
			Padding = new Thickness(30),
			Children = { _setNullButton, _addContentButton, _scrollView }
		};

		// Set the content of the page
		Content = layout;
	}

	void OnSetContentNullClicked(object sender, EventArgs e)
	{
		// Set ScrollView content to null
		_scrollView.Content = null;
	}

	void OnAddContentClicked(object sender, EventArgs e)
	{
		// Restore the original content
		if (_originalContent != null)
		{
			_scrollView.Content = _originalContent;
		}
	}
}