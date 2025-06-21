namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 300011, "DatePicker Format property is not working on windows", PlatformAffected.UWP)]
public class Issue300011 : ContentPage
{
	DatePicker datePicker;
	public Issue300011()
	{
		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		datePicker = new DatePicker
		{
			AutomationId = "DatePicker",
			Format = "D",
			Date = new DateTime(2025, 06, 13)
		};

		var button = new Button
		{
			Text = "Change date",
			AutomationId = "300011Button"
		};
		button.Clicked += ButtonClicked;

		grid.Add(datePicker);
		grid.Add(button, 0, 1);

		Content = grid;
	}



	void ButtonClicked(object sender, EventArgs e)
	{
		datePicker.Date = new DateTime(2025, 06, 14);
	}
}