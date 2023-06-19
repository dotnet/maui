using System;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
	[Category(UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32462, "Crash after a page disappeared if a ScrollView is in the HeaderTemplate property of a ListView", PlatformAffected.Android)]
	public class Bugzilla32462 : TestContentPage // or TestFlyoutPage, etc ...
	{
		[Preserve(AllMembers = true)]
		public class ListViewPage : ContentPage
		{
			public ListViewPage()
			{
				var scrollview = new ScrollView
				{
					Orientation = ScrollOrientation.Horizontal,
					Content = new Label { Text = "some looooooooooooooooooooooooooooooooooooooooooooooooooooooog text" }
				};
				var stacklayout = new StackLayout();
				stacklayout.Children.Add(scrollview);
				string[] list = Enumerable.Range(0, 40).Select(c => $"some text {c}").ToArray();
				var listview = new ListView { AutomationId = "listview", Header = stacklayout, ItemsSource = list };
				Content = listview;

				listview.ScrollTo(list[39], ScrollToPosition.Center, false);
			}
		}

		protected override void Init()
		{
			var button = new Button
			{
				Text = "Click!",
			};
			button.Clicked += (object sender, EventArgs e) => Navigation.PushAsync(new ListViewPage());
			Content = button;
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla36729Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Click!"));
			RunningApp.Tap(q => q.Marked("Click!"));
			RunningApp.WaitForElement(q => q.Marked("listview"));
			RunningApp.WaitForElement(q => q.Marked("some text 35"));
			RunningApp.Back();
		}
#endif
	}
}
