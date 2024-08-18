namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 9796, "[Android]Editor/Entry controls don't raise Completed event consistently",
		PlatformAffected.Android)]
	public partial class Issue9796 : ContentPage
	{
		public Issue9796()
		{
			InitializeComponent();
		}

		override async protected void OnAppearing()
		{
			base.OnAppearing();
			Editor.Focus();
			await Task.Delay(500);
			Entry.Focus();
			await Task.Delay(300);
			Editor.Focus();
			Entry.Focus();
		}
		private void Editor_Completed(object sender, EventArgs e)
		{
			EditorStatusLabel.Text = "Editor Completed by UnFocused";	
		}

		private void Entry_Completed(object sender, EventArgs e)
		{
			EntryStatusLabel.Text = "Entry Completed by UnFocused";
		}
	}
}