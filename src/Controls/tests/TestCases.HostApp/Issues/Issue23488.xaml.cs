namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 23488, "Span text-decoration is incorrect whereas the Label behaves properly", PlatformAffected.UWP)]
	public partial class Issue23488 : ContentPage
    {
        public Issue23488()
        {
            InitializeComponent();
        }
    }
}