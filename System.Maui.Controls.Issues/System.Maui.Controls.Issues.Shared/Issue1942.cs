using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1942, "[Android] Attached Touch Listener events do not dispatch to immediate parent Grid Renderer View on Android when Child fakes handled",
		PlatformAffected.Android)]
	public class Issue1942 : TestContentPage
	{
		public const string SuccessString = "Success";
		public const string ClickMeString = "CLICK ME";

		protected override void Init()
		{
			Content = new CustomGrid()
			{
				Children =
				{
					new Grid
					{
						Children = { new Label() { Text = ClickMeString, BackgroundColor = Color.Blue, HeightRequest = 300, WidthRequest = 300 } }
					}
				}
			};
		}

		public class CustomGrid : Grid { }

#if UITEST && __ANDROID__
		[Test]
		public void ClickPropagatesToOnTouchListener()
		{
			RunningApp.Tap(ClickMeString);
			RunningApp.WaitForElement(SuccessString);
		}
#endif
	}
}
