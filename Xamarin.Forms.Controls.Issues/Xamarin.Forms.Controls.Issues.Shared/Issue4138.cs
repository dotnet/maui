using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
    [Preserve(AllMembers = true)]
    [Issue(IssueTracker.Github, 4138, "[iOS] NavigationPage.TitleIcon no longer centered",
        PlatformAffected.iOS)]
#if UITEST
    [NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
    public class Issue4138 : TestNavigationPage
    {
        protected override void Init()
        {
            ContentPage contentPage = new ContentPage();

            NavigationPage.SetTitleIcon(contentPage, "coffee.png");

            PushAsync(contentPage);
        }


#if UITEST && __IOS__
		[Test]
        public void TitleIconIsCentered()
        {
            var element = RunningApp.WaitForElement("coffee.png")[0];
			var rect = RunningApp.RootViewRect();
			Assert.AreEqual(element.Rect.CenterX, rect.CenterX);
        }
#endif
    }
}