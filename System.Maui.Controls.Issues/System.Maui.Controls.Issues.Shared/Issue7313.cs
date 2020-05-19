using System;
using System.Threading.Tasks;
using System.Maui;
using System.Maui.CustomAttributes;
using System.Maui.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using System.Maui.Core.UITests;
#endif

namespace System.Maui.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7313, "ListView RefreshControl Not Hiding", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue7313 : TestContentPage
	{
		ListView _listView;
		Label _testLoaded;
		string _testReady = "If you see the refresh circle this test has failed";

		protected override void Init()
		{
			_listView = new ListView
			{
				BackgroundColor = Color.Transparent,
				IsPullToRefreshEnabled = true,
				RefreshControlColor = Color.Cyan,
				ItemsSource = new []{ "ListLoaded" }
			};

			_testLoaded = new Label();
			Content = new StackLayout()
			{
				Children =
				{
					_testLoaded,
					_listView
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			_listView.IsRefreshing = true;
			await Task.Delay(1);
			_listView.IsRefreshing = false;
			await Task.Delay(1);
			_testLoaded.Text = _testReady;

		}

#if UITEST && __IOS__
		[Test]
		public void RefreshControlTurnsOffSuccessfully()
		{
			RunningApp.WaitForElement(_testReady);

			RunningApp.WaitForNoElement("RefreshControl");
		}
#endif

	}
}
