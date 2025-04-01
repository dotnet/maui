namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23070, "[iOS] Border redraws with 1 frame lag", PlatformAffected.iOS)]

public partial class Issue23070 : ContentPage
{
	int t = 0;

	public Issue23070()
	{
		InitializeComponent();
		var template = (DataTemplate)Resources[$"T0"];
		TheGrid.Add((View)template.CreateContent(), 0, 1);
	}

	private async void ButtonClicked(object sender, EventArgs e)
	{
		TheButton.IsEnabled = false;

		TheGrid.RemoveAt(1);
		t = (t + 1) % 2;
		var template = (DataTemplate)Resources[$"T{t}"];
		var content = (View)template.CreateContent();
		content.IsVisible = false;
		TheGrid.Add(content, 0, 1);
		await Task.Delay(1000);
		content.IsVisible = true;
		await Task.Delay(1000);
		content.IsVisible = false;
		await Task.Delay(1000);
		content.IsVisible = true;

		TheButton.IsEnabled = true;
	}
}
