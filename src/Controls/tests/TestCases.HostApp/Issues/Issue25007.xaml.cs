namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25007, "ContentPage Property InputTransparent = true, causes blank page", PlatformAffected.Android)]
	public partial class Issue25007 : ContentPage
	{
		int _clicksCount;
		public Issue25007()
		{
			InitializeComponent();
		}

		private void ButtonClicked(object sender, EventArgs e)
		{
			this.InputTransparent = true;
		}

		private void UpdateCountClicked(object sender, EventArgs e)
		{
			ActionButton.Text = $"Count: {++_clicksCount}";
		}
	}
}
