namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 6963, "[Bug] CollectionView multiple pre-selection throws ArgumentOutOfRangeException when SelectedItems is bound to an ObservableCollection initialized inside the constructor.",
		PlatformAffected.iOS | PlatformAffected.UWP)]
	public class Issue6963 : NavigationPage
	{
		public Issue6963() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new CollectionViewGalleries.SelectionGalleries.SelectionSynchronization());
			}
		}
	}
}