namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24511, "Spacing in the ItemsLayout of CollectionView stops it from scrolling", PlatformAffected.Android)]
	public partial class Issue24511 : ContentPage
	{
		public Issue24511()
		{
			InitializeComponent();
			CV.ItemsSource = Enumerable.Range(0, 100).Select(x => $"Item{x}").ToList();
		}
	}
}