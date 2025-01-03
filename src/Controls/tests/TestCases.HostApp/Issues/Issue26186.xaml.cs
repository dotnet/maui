namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 26186, "When dynamically changing the CollectionView's width, the end portion of the CollectionView header's and footer's children is cut off", PlatformAffected.iOS | PlatformAffected.macOS)]
public partial class Issue26186 : ContentPage
{
	public Issue26186()
	{
		InitializeComponent();
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		collectionView.WidthRequest = 300;
	}
}