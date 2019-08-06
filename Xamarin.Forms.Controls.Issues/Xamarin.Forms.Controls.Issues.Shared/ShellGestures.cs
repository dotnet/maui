using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using System.Threading;
using System.ComponentModel;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Gestures Test",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellGestures : TestShell
	{
		const string Success = "Success";
		const string SuccessId = "SuccessId";

		protected override void Init()
		{
			var gesturePage = CreateContentPage(shellItemTitle: "Gestures");

			var label = new Label()
			{
				Text = "Swipe Right and Text Should Change to Success",
				AutomationId = SuccessId
			};

			gesturePage.Content = new StackLayout()
			{
				Children =
				{
					new Label(){ Text = "Click through flyout items for all the tests"},
					label
				},
				GestureRecognizers =
				{
					new SwipeGestureRecognizer()
					{
						Direction = SwipeDirection.Right,
						Command = new Command(() =>
						{
							label.Text = Success;
						})
					}
				}
			};
		}


#if UITEST && (__IOS__ || __ANDROID__)
		[Test]
		public void GesturesTest()
		{
			RunningApp.WaitForElement(SuccessId);
			RunningApp.SwipeLeftToRight(SuccessId);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
