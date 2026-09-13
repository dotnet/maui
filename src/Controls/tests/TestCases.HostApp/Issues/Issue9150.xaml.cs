namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 9150, "Picker SelectedIndex set before items should initialize selection", PlatformAffected.All)]
public partial class Issue9150 : ContentPage
{
	public static IList<string> StaticItems { get; } = new List<string> { "Alpha", "Beta", "Gamma" };

	int _selectedIndexChangedCount;
	bool _initialized;

	public Issue9150()
	{
		InitializeComponent();
		_initialized = true;
		UpdateStatus();
	}

	void OnSelectedIndexChanged(object sender, EventArgs e)
	{
		_selectedIndexChangedCount++;

		if (_initialized)
			UpdateStatus();
	}

	void UpdateStatus()
	{
		var passed =
			ItemsSourcePicker.SelectedIndex == 1 &&
			ItemsSourcePicker.SelectedItem as string == "Beta" &&
			InlineItemsPicker.SelectedIndex == 2 &&
			InlineItemsPicker.SelectedItem as string == "Blue" &&
			_selectedIndexChangedCount == 0;

		StatusLabel.Text = passed
			? "Passed"
			: $"Failed: ItemsSource=({ItemsSourcePicker.SelectedIndex}, {ItemsSourcePicker.SelectedItem}), Inline=({InlineItemsPicker.SelectedIndex}, {InlineItemsPicker.SelectedItem}), Events={_selectedIndexChangedCount}";
	}
}
