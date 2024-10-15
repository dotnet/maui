namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 20294, "CollectionView containing a Footer and a Border with StrokeThickness set to decimal value crashes on scroll", PlatformAffected.iOS)]
	public partial class Issue20294 : ContentPage
	{
		public Issue20294()
		{
			InitializeComponent();
		}
	}
}