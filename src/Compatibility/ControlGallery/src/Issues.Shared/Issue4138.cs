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
	[Issue(IssueTracker.Github, 4138, "[iOS] NavigationPage.TitleIcon no longer centered",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	public class Issue4138 : TestNavigationPage
	{
		protected override void Init()
		{
			ContentPage contentPage = new ContentPage();

			NavigationPage.SetTitleIconImageSource(contentPage, "coffee.png");

			PushAsync(contentPage);
		}


#if UITEST && __IOS__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TitleIconIsCentered()
		{
			var element = RunningApp.WaitForElement("coffee.png")[0];
			var rect = RunningApp.RootViewRect();
			Assert.AreEqual(element.Rect.CenterX, rect.CenterX);
		}
#endif
	}
}