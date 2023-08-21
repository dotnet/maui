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

using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 29128, "Slider background lays out wrong Android")]
	public class Bugzilla29128 : TestContentPage
	{
		protected override void Init()
		{
			Content = new Slider
			{
				AutomationId = "SliderId",
				BackgroundColor = Colors.Blue,
				Maximum = 255,
				Minimum = 0,
			};
		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla29128Test()
		{
			RunningApp.WaitForElement(q => q.Marked("SliderId"));
			RunningApp.Screenshot("Slider and button should be centered");
			Assert.Inconclusive("For visual review only");
		}
#endif
	}
}
