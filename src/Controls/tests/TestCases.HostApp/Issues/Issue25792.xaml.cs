using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 25792, "Picker ItemsSource Change Triggers Exception: 'Value Does Not Fall Within the Expected Range", PlatformAffected.UWP)]
	public partial class Issue25792 : ContentPage
	{
		private Picker _picker;
		public Issue25792()
		{
			_picker = new Picker
			{
				Title = "Select an Item",
				ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3", "Item 4" },
				SelectedIndex = 3, // Pre-select the last item in the initial data source
				AutomationId = "Picker"
			};

			var changeSourceButton = new Button
			{
				Text = "Change Data Source",
				AutomationId = "ChangeDataButton"
			};
			changeSourceButton.Clicked += ChangeSourceButton_Clicked;

			Content = new StackLayout
			{
				Children = { _picker, changeSourceButton }
			};
		}
		private void ChangeSourceButton_Clicked(object sender, EventArgs e)
		{
			_picker.ItemsSource = new List<string> { "New Item 1", "New Item 2" };
		}
	}
}
