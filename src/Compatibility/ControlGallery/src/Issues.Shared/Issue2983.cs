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
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2983, "ListView.Footer can cause NullReferenceException", PlatformAffected.iOS)]
	public class Issue2983 : TestContentPage
	{
		protected override void Init()
		{
			Content = new ListView
			{
				Footer = new StackLayout
				{
					Children = { new Label { Text = "Footer", AutomationId = "footer" } }
				}
			};
		}

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void TestDoesNotCrash()
		{
			RunningApp.WaitForElement(c => c.Marked("footer"));
		}
#endif
	}
}