using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31375, "[Windows] RefreshView Command executes multiple times when IsRefreshing is set to True", PlatformAffected.UWP)]
public class Issue31375 : ContentPage
{
	bool _isLoading = false;
	Label _textLabel;
	int count = 0;

	public bool IsLoading
	{
		get => _isLoading;
		set
		{
			_isLoading = value;
			OnPropertyChanged(nameof(IsLoading));
		}
	}

	public ICommand RefreshCommand { get; set; }
	public Issue31375()
	{
		// Create UI elements
		var button = new Button { Text = "Click To Refresh", AutomationId = "RefreshButton" };
		button.Clicked += Button_Clicked;

		_textLabel = new Label() { AutomationId = "CounterLabel" };
		_textLabel.Text = count++.ToString();

		// Set BindingContext
		BindingContext = this;

		// Assign command
		RefreshCommand = new Command(AddItems);
		var refreshView = new RefreshView
		{
			Margin = 16,
			Content = _textLabel,
		};
		refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(IsLoading));
		refreshView.SetBinding(RefreshView.CommandProperty, nameof(RefreshCommand));
		Content = new StackLayout
		{
			Children = { refreshView, button }
		};
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		IsLoading = true;
	}

	void AddItems()
	{
		IsLoading = false;
		_textLabel.Text = count++.ToString();
	}
}