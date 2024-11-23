namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25983, "Issue25983 Grid not getting invalidated when changing the Height/Width of Row/ColumnDefinitions declared with the short syntax", PlatformAffected.All)]
	public partial class Issue25983 : ContentPage
	{
		public Issue25983()
		{
			InitializeComponent();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			this.grid.RowDefinitions[2].Height = new GridLength(this.grid.RowDefinitions[2].Height.Value + 100, GridUnitType.Absolute);
		}

		private void Button2_Clicked(object sender, EventArgs e)
		{
			this.grid.ColumnDefinitions[1].Width = new GridLength(this.grid.ColumnDefinitions[1].Width.Value + 100, GridUnitType.Absolute);
		}
	}
}