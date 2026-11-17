namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39693, "OrientationStateTriggers are broken with iOS 27", PlatformAffected.iOS)]
public partial class Issue39693 : ContentPage
{
	public Issue39693()
	{
		InitializeComponent();
		Loaded += (_, _) => UpdateStatus();
		controlsGrid.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(BackgroundColor) ||
				e.PropertyName == nameof(Grid.RowDefinitions) ||
				e.PropertyName == nameof(Grid.ColumnDefinitions))
				UpdateStatus();
		};
	}

	void UpdateStatus() => Dispatcher.Dispatch(() =>
	{
		var color = controlsGrid.BackgroundColor;
		var colorName = color == Colors.Green ? "Green" : color == Colors.Black ? "Black" : "Orange";
		Status.Text = $"{colorName}; rows={controlsGrid.RowDefinitions.Count}; columns={controlsGrid.ColumnDefinitions.Count}";
	});
}
