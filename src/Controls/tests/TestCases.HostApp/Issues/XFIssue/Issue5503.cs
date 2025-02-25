using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5503, "[iOS] UITableView.Appearance.BackgroundColor ignored or overridden for ListView",
	PlatformAffected.iOS)]
public class Issue5503 : TestNavigationPage
{
	const string ChangeBackgroundButtonAutomationId = "ChangeBackgroundButton";
	const string ListViewAutomationId = "TheListView";

	public static string ChangeUITableViewAppearanceBgColor = "BIBBIDYBOBBIDIBOOOO";

	ObservableCollection<string> _items = new ObservableCollection<string>();
	public ObservableCollection<string> Items
	{
		get => _items;
		set
		{
			_items = value;
			OnPropertyChanged();
		}
	}

	protected override void Init()
	{
		BindingContext = this;

		var listView = new ListView
		{
			AutomationId = ListViewAutomationId
		};

		listView.SetBinding(ItemsView<Cell>.ItemsSourceProperty, new Binding(nameof(Items)));

		for (int i = 0; i < 100; i++)
		{
			Items.Add($"Item {i}");
		}

		Padding = new Thickness(10);
		BackgroundColor = Colors.LightGray;

		var changeAppearanceButton = new Button()
		{
			Text = "Change Background through Appearance API",
			AutomationId = ChangeBackgroundButtonAutomationId
		};

		changeAppearanceButton.Clicked += (s, a) =>
		{
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Send(this, ChangeUITableViewAppearanceBgColor);
#pragma warning restore CS0618 // Type or member is obsolete
		};

		var stack = new StackLayout()
		{
			changeAppearanceButton,
			listView
		};

		var button = new Button() { Text = "Go To Test Page" };
		button.Clicked += (sender, args) =>
		{
			Navigation.PushAsync(new ContentPage() { Content = stack });
		};

		var content = new ContentPage() { Content = button };
		Navigation.PushAsync(content);

	}
}
