using System;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2241, "ScrollView content can become stuck on orientation change (iOS)", PlatformAffected.iOS)]
	public class Issue2241 : TestContentPage
	{
		protected override void Init ()
		{
			ScrollView scrollView = new ScrollView () {
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				Padding = new Thickness (20),
				Content = new BoxView () {
					Color = Color.Red,
					HeightRequest = 400,
					HorizontalOptions = LayoutOptions.FillAndExpand
				}
			};

			Content = scrollView;
		}

#if UITEST
		[Test]
		[Ignore("Fails intermittently on TestCloud")]
		public void ChangeOrientationCheckScroll ()
		{
#if __ANDROID__
			var isAndroid = true;
#else
			var isAndroid = false;
#endif
			var className = "Xamarin_Forms_Platform_iOS_BoxRenderer";
			if (isAndroid) {
				className = "BoxRenderer";
			}
			var box1 = RunningApp.Query (c => c.Class (className)) [0];
			RunningApp.SetOrientationLandscape ();
			RunningApp.ScrollDown ();
			RunningApp.SetOrientationPortrait ();
			var box2 = RunningApp.Query (c => c.Class (className)) [0];
			RunningApp.Screenshot ("Did it resize ok? Do you see some white on the bottom?");
			if (!isAndroid) {
				Assert.AreEqual (box1.Rect.CenterY, box2.Rect.CenterY);
			}
		}
#endif
	}
}


