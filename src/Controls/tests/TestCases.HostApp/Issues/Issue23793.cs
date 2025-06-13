namespace Maui.Controls.Sample.Issues;
	[Issue(IssueTracker.Github, 23793, "DatePicker Format property is not working on windows", PlatformAffected.UWP)]
public class Issue23793 : ContentPage
{
	DatePicker datePicker;
	public Issue23793()
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
			AutomationId = "Button"
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