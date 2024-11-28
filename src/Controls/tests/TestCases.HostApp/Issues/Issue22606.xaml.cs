namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22606, "Border does not expand on Content size changed", PlatformAffected.All)]
	public partial class Issue22606 : ContentPage
	{
		public Issue22606()
		{
			InitializeComponent();
		}

		void OnSetHeightTo200Clicked(object sender, EventArgs e)
		{
			content.HeightRequest = 200;
		}

		void OnSetHeightTo500Clicked(object sender, EventArgs e)
		{
			content.HeightRequest = 500;
		}
	}
}