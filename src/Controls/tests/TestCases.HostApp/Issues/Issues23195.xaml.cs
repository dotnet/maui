namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23195, "NavigationBarColors from NavigationPage not changing on AppTheme changing", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue23195 : NavigationPage
	{
		public Issue23195()
		{
			InitializeComponent();
			PushAsync(contentPage);
		}

		public void Button_Clicked(object sender, EventArgs eventArgs)
		{
			Application.Current!.UserAppTheme = AppTheme.Dark;
		}
	}
}