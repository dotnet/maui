using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{

#if UITEST
	[Category(UITestCategories.Maps)]
	[Category(UITestCategories.ManualReview)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
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
				BackgroundColor = Color.LightBlue,
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