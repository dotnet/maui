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
	[Issue(IssueTracker.Github, 4782, "[Android] Null drawable crashes Image Button",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.ImageButton)]
#endif
	public class Issue4782 : TestContentPage
	{
		const string _success = "Success";
		public class Issue4782ImageButton : ImageButton { }

		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "If app didn't crash then test passed",
						AutomationId = _success
					},
					new Issue4782ImageButton()
				}
			};
		}

#if UITEST && __ANDROID__
		[Test]
		public void ImageButtonNullDrawable()
		{
			RunningApp.WaitForElement(_success);
		}
#endif
	}
}
