using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7311, "[Bug] [Android] Error back hardware button with Picker", PlatformAffected.Android)]
	public class Issue7311 : TestContentPage
	{
		const string FirstPickerItem = "Uno";
		const string PickerId = "CaptainPickard";
		readonly string[] _items = { FirstPickerItem, "Dos", "Tres" };

		protected override void Init()
		{
			var picker = new Picker
			{
				ItemsSource = _items,
				AutomationId = PickerId
			};

			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Open Picker. Click hardware back button to close picker. Click hardware button a second time and it should navigate back to gallery"
					},
					picker
				}
			};
		}

#if UITEST && __ANDROID__
		[Test]
		public void OpeningPickerPressingBackButtonTwiceShouldNotOpenPickerAgain()
		{
			RunningApp.WaitForElement(PickerId);
			RunningApp.Tap(PickerId);

			RunningApp.WaitForElement(FirstPickerItem);

			RunningApp.Back();

			RunningApp.WaitForNoElement(FirstPickerItem);

			RunningApp.Back();

			RunningApp.WaitForNoElement(FirstPickerItem, "Picker is again visible after second back button press", TimeSpan.FromSeconds(10));

			RunningApp.Screenshot("Back at the previous page, not showing the picker again");
		}
#endif
	}
}