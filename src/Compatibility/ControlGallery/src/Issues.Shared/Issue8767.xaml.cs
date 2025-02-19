using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8767, "SwipeView SwipeBehaviorOnInvoked RemainOpen issue on iOS",
		PlatformAffected.iOS)]
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	public sealed partial class Issue8767 : TestContentPage
	{
		public Issue8767()
		{
#if APP
			this.InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}

#if APP
		void OnSwipeViewBehaviorChanged(object sender, EventArgs e)
		{
			swipeView1.LeftItems.SwipeBehaviorOnInvoked = (SwipeBehaviorOnInvoked)(sender as EnumPicker).SelectedItem;
			swipeView2.LeftItems.SwipeBehaviorOnInvoked = (SwipeBehaviorOnInvoked)(sender as EnumPicker).SelectedItem;
		}

		async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("SwipeView", "Delete invoked.", "OK");
		}

		async void OnFavoriteSwipeItemInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("SwipeView", "Favorite invoked.", "OK");
		}
#endif
	}
}