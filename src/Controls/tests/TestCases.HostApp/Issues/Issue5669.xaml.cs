namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 5669, "Windows SearchBar MaxLength > 0 not working properly", PlatformAffected.UWP)]
	public partial class Issue5669 : ContentPage
	{
		public Issue5669()
		{
			InitializeComponent();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			searchbar.MaxLength = 4;
		}
	}
}