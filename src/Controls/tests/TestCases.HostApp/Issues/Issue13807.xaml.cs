namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 13807, "NavigationPage TitleBar Persists if child pages have a title", PlatformAffected.All)]
	public partial class Issue13807 : TabbedPage
    {
        public Issue13807()
        {
            InitializeComponent();
        }
    }
}