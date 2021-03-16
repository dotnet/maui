
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	public partial class VisualControlsPage : TestShell
	{
		public VisualControlsPage()
		{
#if APP
			InitializeComponent();
			Device.BeginInvokeOnMainThread(OnAppearing);
#endif
		}

		protected override void Init()
		{
			BindingContext = this;
		}

		[Preserve(AllMembers = true)]
		[Issue(IssueTracker.Github, 4435, "Visual Gallery Loads",
			PlatformAffected.iOS | PlatformAffected.Android)]
#if UITEST
		[NUnit.Framework.Category(UITestCategories.Visual)]
#endif
		public class Issue4435 : VisualControlsPage
		{
			protected override void Init()
			{
			}

#if UITEST && !__WINDOWS__
			[Test]
			public void LoadingVisualGalleryPageDoesNotCrash()
			{
				RunningApp.WaitForElement("Activity Indicators");
			}

			[Test]
			[NUnit.Framework.Category(UITestCategories.ManualReview)]
			public void DisabledButtonTest()
			{
				TapInFlyout("Disabled Button Test");
				RunningApp.WaitForElement("If either button looks odd this test has failed.");
				RunningApp.Screenshot("If either button looks off (wrong shadow, border drawn inside button) the test has failed.");
			}
#endif
		}
	}
}
