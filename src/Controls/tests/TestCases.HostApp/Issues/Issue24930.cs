namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24930, "The picker allows you to write text if the keyboard is visible", PlatformAffected.Android)]
	public partial class Issue24930 : ContentPage
	{
		public Issue24930()
		{			
			Picker picker = new Picker 
			{ 
				AutomationId = "picker",
				Title = "Select a monkey"
			};

			
			picker.ItemsSource = new List<string>
			{
				"Baboon",
				"Capuchin Monkey"
			};

			
			StackLayout stackLayout = new StackLayout();
			stackLayout.Children.Add(picker);

			
			Content = new StackLayout()
			{ 
				Children = { picker } 
			};
		}
	}
}