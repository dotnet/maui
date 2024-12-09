namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24930, "The picker allows you to write text if the keyboard is visible", PlatformAffected.Android)]
	public partial class Issue24930 : ContentPage
	{
		const string FirstPickerItem = "Baboon";
		const string PickerId = "picker";
		public Issue24930()
		{
			Picker picker = new Picker
			{
				AutomationId = PickerId,
				Title = "Select a monkey"
			};


			picker.ItemsSource = new List<string>
			{
				FirstPickerItem,
				"Capuchin Monkey"
			};

			Content = new StackLayout()
			{
				Children = { picker }
			};
		}
	}
}