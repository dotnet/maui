namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 26593, "CheckBox UI Test", PlatformAffected.iOS)]
public partial class CheckBoxUITestSample : ContentPage
{
	public CheckBoxUITestSample()
	{
		Grid grid = new Grid()
		{
			ColumnDefinitions = new ColumnDefinitionCollection
			{
				new ColumnDefinition(),
				new ColumnDefinition()
			}
		};

		Label label = new Label
		{
			Text = "CheckBox",
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.End,
			AutomationId = "Label"
		};

		CheckBox checkBox = new CheckBox
		{
			IsChecked = true,
			Color = Colors.Red,
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "CheckBox"
		};

		checkBox.CheckedChanged += (s, e) =>
		{
			label.Text = $"CheckBox is {(checkBox.IsChecked ? "Checked" : "Unchecked")}";
		};

		grid.AddChild(label, 0, 0);
		grid.AddChild(checkBox, 1, 0);
		Content = grid;
	}
}