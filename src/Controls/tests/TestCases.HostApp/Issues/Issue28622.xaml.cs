namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28622, "[Android] CollectionView Header and Footer Do Not Align with Horizontal ItemsLayout When EmptyView is Displayed", PlatformAffected.Android)]
public partial class Issue28622 : ContentPage
{
	public Issue28622()
	{
		InitializeComponent();
	}

	private void OnLayoutButtonClicked(object sender, EventArgs e)
	{
		collectionView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
	}

	private void OnHeaderButtonClicked(object sender, EventArgs e)
	{
		collectionView.Header = "CollectionView Header(String)";
	}

	private void OnFooterButtonClicked(object sender, EventArgs e)
	{
		collectionView.Footer = "CollectionView Footer(String)";
	}

	private void OnEmptyViewButtonClicked(object sender, EventArgs e)
	{
		collectionView.EmptyView = "EmptyView String";
	}
}