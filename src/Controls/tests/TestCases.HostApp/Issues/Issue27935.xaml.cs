namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27935, "iOS: Rectangle that is invisible when page loads can never be made visible", PlatformAffected.iOS)]
	public partial class Issue27935 : ContentPage
	{
		public Issue27935()
		{
			InitializeComponent();
		}

		private void OnButtonClick(object sender, EventArgs e)
		{
			rectangle.IsVisible = !rectangle.IsVisible;
			roundRectangle.IsVisible = !roundRectangle.IsVisible;
		}
	}
}