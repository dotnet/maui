namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 9929, "[Bug] NSInternalInconsistencyException when trying to run XamarinTV on iOS",
		PlatformAffected.iOS)]
	public class Issue9929 : NavigationPage
	{
		public Issue9929() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new CollectionViewGalleries.SpacingGalleries.SpacingGallery(new GridItemsLayout(3, ItemsLayoutOrientation.Vertical)));
			}
		}
	}
}