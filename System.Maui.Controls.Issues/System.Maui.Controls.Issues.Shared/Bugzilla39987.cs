using System;

using System.Maui.CustomAttributes;
using System.Maui.Internals;
using System.Maui.Maps;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39987, "Bug 39987 - MapView not working correctly on iOS 9.3")]
	public class Bugzilla39987 : TestTabbedPage // or TestMasterDetailPage, etc ...
	{
		const string TabTitle = "Test";
		const string TestMap = "Map";
		const string Ok = "Ok";
		protected override void Init()
		{
			for (int i = 1; i <= 3; i++)
				Children.Add(new CustomMapPage($"{TabTitle} {i}"));
		}
		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await DisplayAlert("Instructions", "Navigating for all tabs, if don't crash the test passed", Ok);
		}
		[Preserve(AllMembers = true)]
		public class CustomMapView : View
		{
			public CustomMapView()
			{

			}

		}

		[Preserve(AllMembers = true)]
		public class CustomMapPage : ContentPage
		{
			private CustomMapView _customMapView;

			public CustomMapPage(string title)
			{
				var map = new Map
				{
					AutomationId = TestMap
				};
				Title = title;
				Content = map;
			}
			public CustomMapPage(CustomMapView customMapView, string title)
			{
				Title = title;
				_customMapView = customMapView;
				_customMapView.HorizontalOptions = LayoutOptions.FillAndExpand;
				_customMapView.VerticalOptions = LayoutOptions.FillAndExpand;
				Content = _customMapView;
			}

		}


#if UITEST && __IOS__
		[Test]
		public void MapViewInTabbedPage()
		{
			RunningApp.WaitForElement(Ok);
			RunningApp.Tap(Ok);
			for (int i = 1; i <= 3; i++)
			{
				RunningApp.WaitForElement(TestMap);
				RunningApp.Tap($"{TabTitle} {i}"); 
			}
		}
#endif

	}
}
