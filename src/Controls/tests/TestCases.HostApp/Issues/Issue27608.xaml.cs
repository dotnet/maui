namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27608, "Items shapes are sometimes rendered incorrectly using CollectionView2Handler", PlatformAffected.iOS)]
	public partial class Issue27608 : ContentPage
	{
		public Issue27608()
		{
			InitializeComponent();
			const int count = 50;
			Items = Enumerable.Range(1, count).ToList();
			BindingContext = this;
		}
		public List<int> Items { get; }
	}
}