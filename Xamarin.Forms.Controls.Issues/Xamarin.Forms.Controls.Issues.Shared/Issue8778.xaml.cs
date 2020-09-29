using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.SwipeView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8778, "[Bug] SwipeViewItem handler invocation intermittent issue on iOS and Android", PlatformAffected.Android)]
	public partial class Issue8778 : ContentPage
	{
		public Issue8778()
		{
#if APP
			InitializeComponent();
#endif
		}

#if APP
		void OnSwipeStarted(object sender, SwipeStartedEventArgs e)
		{
			Console.WriteLine($"\tSwipeStarted: Direction - {e.SwipeDirection}");
		}

		void OnSwipeChanging(object sender, SwipeChangingEventArgs e)
		{
			Console.WriteLine($"\tSwipeChanging: Direction - {e.SwipeDirection}, Offset - {e.Offset}");
		}

		void OnSwipeEnded(object sender, SwipeEndedEventArgs e)
		{
			Console.WriteLine($"\tSwipEnded: Direction - {e.SwipeDirection}");
		}

		void OnCloseRequested(object sender, EventArgs e)
		{
			Console.WriteLine("\tCloseRequested.");
		}

		void OnCloseButtonClicked(object sender, EventArgs e)
		{
			swipeView.Close();
		}

		async void OnFavoriteSwipeItemInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("SwipeView", "Favorite invoked.", "OK");
		}

		async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("SwipeView", "Delete invoked.", "OK");
		}
#endif
	}
}