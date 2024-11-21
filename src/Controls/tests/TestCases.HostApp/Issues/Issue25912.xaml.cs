namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 25912, "ToolbarItem color when used with IconImageSource is always white", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue25912 : ContentPage
	{

		public Issue25912()
		{
			InitializeComponent();
		}
	}
}