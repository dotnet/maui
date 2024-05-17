using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
	[Issue(IssueTracker.Github, 7240, "[Android] Shell content layout hides navigated to page",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue7240 : TestShell
	{
		const string Success = "Page Count:3";
		const string ClickMe = "ClickMe";

		int pageCount = 1;
		protected override void Init()
		{
			Func<ContentPage> createNewPage = null;
			createNewPage = () =>
				   new ContentPage()
				   {
					   Content = new StackLayout()
					   {
						   Children =
						   {
								new Button()
								{
									Text = "Click me and you should see a new page with this same button in the same place",
									AutomationId = ClickMe,
									Command = new Command(() =>
									{
										pageCount++;
										Navigation.PushAsync(createNewPage());
									})
								},
								new Label()
								{
									Text = $"Page Count:{pageCount}"
								}
						   }
					   }
				   };

			AddContentPage(createNewPage());
		}

#if UITEST && (__IOS__ || __ANDROID__)
		[Test]
		public void ShellSecondPageHasSameLayoutAsPrimary()
		{
			RunningApp.WaitForElement(ClickMe);
			RunningApp.Tap(ClickMe);
			RunningApp.WaitForElement("Page Count:2");
			RunningApp.Tap(ClickMe);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
