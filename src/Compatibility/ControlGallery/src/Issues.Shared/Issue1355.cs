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
#if UITEST
	[Category(UITestCategories.Navigation)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1355, "Setting Main Page in quick succession causes crash on Android",
		PlatformAffected.Android)]
	public class Issue1355 : TestContentPage
	{
		int _runCount = 0;
		int _maxRunCount = 2;
		const string Success = "Success";

		protected override void Init()
		{
			Appearing += OnAppearing;
		}

		private void OnAppearing(object o, EventArgs eventArgs)
		{
			Application.Current.MainPage = CreatePage();
		}

		ContentPage CreatePage()
		{
			var page = new ContentPage
			{
				Content = new Label { Text = Success },
				Title = $"CreatePage Iteration: {_runCount}"
			};

			page.Appearing += (sender, args) =>
			{
				_runCount += 1;
				if (_runCount <= _maxRunCount)
				{
					Application.Current.MainPage = new NavigationPage(CreatePage());
				}
			};

			return page;
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public void SwitchMainPageOnAppearing()
		{
			// Without the fix, this would crash. If we're here at all, the test passed.
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
