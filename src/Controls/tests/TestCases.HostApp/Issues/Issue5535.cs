namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 5535, "CollectionView: Swapping EmptyViews has no effect",
		PlatformAffected.iOS | PlatformAffected.Android)]
	public class Issue5535 : NavigationPage
	{
		public Issue5535() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new CollectionViewGalleries.EmptyViewGalleries.EmptyViewSwapGallery());
			}
		}
	}
}