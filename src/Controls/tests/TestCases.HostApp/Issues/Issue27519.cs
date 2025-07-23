namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27519, "When opening the Picker, the first item is selected instead of the currently selected item", PlatformAffected.macOS)]
	public class Issue27519 : ContentPage
	{
		public Issue27519()
		{
			var items = new List<string>
			{
				"1: First", "2: Second", "3: Third", "4: Fourth", "5: Fifth" ,
				"6: Sixth", "7: Seventh", "8: Eighth", "9: Ninth", "10: Tenth"
			};

			Content = new Picker()
			{
				AutomationId = "Picker",
				ItemsSource = items,
				SelectedItem = items[4]
			};
		}
	}
}
