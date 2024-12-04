namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26083, "UI not updating GridItemsLayout when Span becomes 1", PlatformAffected.UWP)]
public partial class Issue26083 : ContentPage
{
	public Issue26083()
	{
		InitializeComponent();
	}

	private void CollectionView_SizeChanged(object sender, EventArgs e)
	{
		CollectionView collectionView = sender as CollectionView;
		if (collectionView != null)
		{
			GridItemsLayout gridItemsLayout = (GridItemsLayout)collectionView.ItemsLayout;
			if (collectionView.Width < 1000)
			{
				gridItemsLayout.Span = 1;
			}
			else
			{
				gridItemsLayout.Span = 2;
			}

		}

	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		customGrid.WidthRequest = 100;
	}

	public class MainViewModel
	{
		public List<string> Items { get; set; }
		public MainViewModel()
		{
			Items = new List<string>()
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5"
			};
		}
	}
}


