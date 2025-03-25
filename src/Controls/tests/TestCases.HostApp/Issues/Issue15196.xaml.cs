namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 15196, "Nested Entry View In A Frame Causes Crash", PlatformAffected.Android)]
	public partial class Issue15196 : ContentPage
	{
		public Issue15196()
		{
			InitializeComponent();
		}

		private void OnButtonClicked(object sender, EventArgs e)
		{
			if (stackLayout.Children.Contains(frame))
			{
				stackLayout.Children.Remove(frame);
			}

			frame?.Handler?.DisconnectHandler();
			entry?.Handler?.DisconnectHandler();
		}
	}
}