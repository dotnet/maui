using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Graphics;
#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{

#if UITEST
	[Category(UITestCategories.Maps)]
	[Category(UITestCategories.ManualReview)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 1701, "Modal Page over Map crashes application", PlatformAffected.Android)]
	public class MapsModalCrash : TestContentPage
	{
		const string StartTest = "Start Test";
		const string DisplayModal = "Click Me";
		const string Success = "SuccessLabel";

		protected override void Init()
		{
			var button = new Button { Text = StartTest };
			button.Clicked += (sender, args) =>
			{
				Application.Current.MainPage = MapPage();
			};

			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children =
				{
					button
				}
			};
		}

		static ContentPage MapPage()
		{
			var map = new Map();

			var button = new Button { Text = DisplayModal };
			button.Clicked += (sender, args) => button.Navigation.PushModalAsync(new NavigationPage(SuccessPage()));

			return new ContentPage
			{
				Content = new StackLayout
				{
					Children =
					{
						map,
						button
					}
				}
			};
		}

		static ContentPage SuccessPage()
		{
			return new ContentPage
			{
				BackgroundColor = Colors.LightBlue,
				Content = new Label { Text = "If you're seeing this, then the test was a success.", AutomationId = Success }
			};
		}

#if UITEST
		[Test]
		public void CanDisplayModalOverMap()
		{
			RunningApp.WaitForElement(StartTest);
			RunningApp.Tap(StartTest);
			RunningApp.WaitForElement(DisplayModal);
			RunningApp.Tap(DisplayModal);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}