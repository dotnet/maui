using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
	[Issue(IssueTracker.Github, 7290, "[Android] DisplayActionSheet or DisplayAlert in OnAppearing does not work on Shell",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue7290 : TestShell
	{
		protected override void Init()
		{
			ContentPage displayAlertPage = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label{ Text = "If you did not see an alert this test has failed."},
					}
				},
				Title = "Display Alert"
			};

			displayAlertPage.Appearing += async (_, __) =>
			{
				await displayAlertPage.DisplayAlert("Title", "Close Alert", "Cancel");
				this.CurrentItem = Items[1];
			};


			ContentPage actionSheetPage = new ContentPage()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label{ Text = "If you did not see an Alert then an Action Sheet this test has failed"},
					}
				},
				Title = "Display Action Sheet"
			};

			actionSheetPage.Appearing += async (_, __) =>
			{
				await actionSheetPage.DisplayActionSheet("Title", "Cancel", "Close Action Sheet", "Button");
			};

			AddContentPage(displayAlertPage);
			AddContentPage(actionSheetPage);
		}

#if UITEST
		[Test]
		public void DisplayActionSheetAndDisplayAlertFromOnAppearing()
		{
			RunningApp.Tap("Cancel");
			RunningApp.Tap("Close Action Sheet");
		}
#endif
	}
}
