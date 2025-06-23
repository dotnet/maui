﻿namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 4600, "[iOS] CollectionView crash with empty ObservableCollection", PlatformAffected.iOS)]
	public class Issue4600 : NavigationPage
	{
		public Issue4600() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new CollectionViewGalleries.ObservableCodeCollectionViewGallery(initialItems: 0));
			}
		}
	}
}