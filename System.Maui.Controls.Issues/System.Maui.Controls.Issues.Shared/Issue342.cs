using System;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 342, "NRE when Image is not assigned source", PlatformAffected.WinPhone)]
	public class Issue342NoSource : TestContentPage
	{
		protected override void Init ()
		{
			Title = "Issue 342";
			Content = new StackLayout {
				Children = {
					new Label {
						Text = "Uninitialized image"
					},
					new Image ()
				}
			};
		}

		// Should not throw exception when user does not include image

#if UITEST
		[Test]
		public void Issue342NoSourceTestsLablePresentNoImage ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Uninitialized image"), "Cannot see label");
			RunningApp.Screenshot ("All elements present");
		}
#endif
    }

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 342, "NRE when Image is delayed source", PlatformAffected.WinPhone)]
	public class Issue342DelayedSource : TestContentPage
	{
		protected override void Init ()
		{
			Title = "Issue 342";

			_image = new Image ();

			Content = new StackLayout {
				Children = { 
					new Label {
						Text = "Delayed image"
					},
					_image 
				}
			};

			AddSourceAfterDelay ();
		}

		// Should not throw exception when user does not include image
		Image _image;

		void AddSourceAfterDelay ()
		{
			Device.StartTimer (TimeSpan.FromSeconds (2), () => {
				_image.Source = "cover1.jpg";
				return false;
			});
		}

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void Issue342DelayedLoadTestsImageLoads ()
		{
			RunningApp.Screenshot ("Should not crash");
		}
#endif
	}
}
