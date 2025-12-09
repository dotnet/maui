namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21325, "Grey cannot be used to set Background property, and doesn't display a preview in the XAML editor", PlatformAffected.All)]
	public partial class Issue21325 : ContentPage
	{
		public Issue21325()
		{
			InitializeComponent();

			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(this, true);
		}
	}
}


