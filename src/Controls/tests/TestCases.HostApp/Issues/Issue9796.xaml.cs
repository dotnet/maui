namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 9796, "[Android]Editor controls don't raise Completed event consistently",
		PlatformAffected.Android)]
	public partial class Issue9796 : ContentPage
	{
		public Issue9796()
		{
			InitializeComponent();
		}

		private void Editor_Completed(object sender, EventArgs e)
		{
			Label.Text = "Triggered";	
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			var Button = (Button)sender;
			if(Button.Text == "Focus")
			{
				Editor.Focus();
			}
		}

		private void UnFocusButton_Clicked(object sender, EventArgs e)
		{
			var Button = (Button)sender;
		    if (Button.Text == "Unfocus")
			{
				Editor.Unfocus();
			}
		}
	}
}
