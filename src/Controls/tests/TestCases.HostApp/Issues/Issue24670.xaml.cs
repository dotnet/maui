namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, "24670", "SearchHandler.Focused event never fires", PlatformAffected.All)]
	public partial class Issue24670 : Shell
	{
		public Issue24670()
		{
			InitializeComponent();
		}

		private void SearchHandler_Focused(object sender, EventArgs e)
		{
			focusedLabel.Text = "Focused: True";
		}

		private void SearchHandler_Unfocused(object sender, EventArgs e)
		{
			unfocusedLabel.Text = "Unfocused: True";
		}
	}
}