using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12848, "[Bug] CarouselView position resets when visibility toggled",
		PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CarouselView)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	public partial class Issue12848 : TestContentPage
	{
		protected override void Init()
		{
#if APP
			InitializeComponent();

			BindingContext = new List<int> { 1, 2, 3 };
#endif
		}

#if APP
		void OnShowButtonClicked(object sender, EventArgs e)
		{
			carousel.IsVisible = true;
		}

		void OnHideButtonClicked(object sender, EventArgs e)
		{
			carousel.IsVisible = false;
		}
#endif

#if UITEST
		[Test]
		[Compatibility.UITests.FailsOnMauiAndroid]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue12848Test()
		{
			RunningApp.WaitForElement("TestCarouselView");
			RunningApp.SwipeRightToLeft();
			Assert.AreEqual(1, int.Parse(RunningApp.Query(q => q.Marked("CarouselPosition"))[0].Text));
			RunningApp.Tap("HideButton");
			RunningApp.Tap("ShowButton");
			Assert.AreEqual(1, int.Parse(RunningApp.Query(q => q.Marked("CarouselPosition"))[0].Text));
			RunningApp.Screenshot("Test passed");
		}
#endif
	}
}