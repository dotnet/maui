namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17228, "Back button image color can't be changed", PlatformAffected.Android)]
	public class Issue17228Shell : Shell
	{
		public Issue17228Shell()
		{
			Routing.RegisterRoute(nameof(Issue17228), typeof(Issue17228));
			Items.Add(new ShellContent()
			{
				Content = new ContentPage() { Title = "Home page" }
			});
			GoToAsync(nameof(Issue17228));
		}
	}

	public partial class Issue17228 : ContentPage
	{
		public Issue17228()
		{
			InitializeComponent();
		}
	}
}