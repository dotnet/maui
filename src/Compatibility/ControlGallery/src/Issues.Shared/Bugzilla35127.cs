//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ScrollView)]
	[Category(UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 35127, "It is possible to craft a page such that it will never display on Windows")]
	public class Bugzilla35127 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children = {
					new Label { Text = "See me?" },
					new ScrollView {
						IsVisible = false,
						AutomationId = "scrollView",
						Content = new Button { Text = "Click Me?" }
					}
				}
			};
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue35127Test()
		{
			RunningApp.WaitForElement(q => q.Marked("See me?"));
			var count = RunningApp.Query(q => q.Marked("scrollView")).Length;
			Assert.IsTrue(count == 0);
			RunningApp.WaitForNoElement(q => q.Marked("Click Me?"));
		}
#endif
	}
}