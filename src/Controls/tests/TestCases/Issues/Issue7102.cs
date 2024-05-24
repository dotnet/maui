using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7102, "[Bug] CollectionView Header cause delay to adding items.",
		PlatformAffected.Android)]
	public class Issue7102 : NavigationPage
	{
		public Issue7102() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new CollectionViewGalleries.ObservableCodeCollectionViewGallery(grid: false));
			}
		}
	}
}