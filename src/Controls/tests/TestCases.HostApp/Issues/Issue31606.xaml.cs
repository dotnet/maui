namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 31606, "Crash on Windows when SemanticProperties.Description is set for ToolbarItem", PlatformAffected.UWP)]
	public partial class Issue31606 : NavigationPage
	{
		public Issue31606()
		{
			Navigation.PushAsync(new Issue31606MainPage());
		}
	}

	public partial class Issue31606MainPage : ContentPage
	{
		public Issue31606MainPage()
		{
			InitializeComponent();
		}
	}
}