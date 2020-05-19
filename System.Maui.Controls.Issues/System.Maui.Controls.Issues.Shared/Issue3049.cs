using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST && __IOS__
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3049, "DisplayActionSheet freezes app in iOS custom renderer (iPad only)", PlatformAffected.iOS)]
	public class Issue3049 : TestContentPage
	{
		const string Button1Id = "button1";
		const string Button2Id = "button2";
		const string LabelId = "label";
		const string Success = "Success";
		const string Action1 = "Don't click me";
		const string Skip = "skip";

		protected override void Init()
		{
			Label instructions = new Label { Text = "Click the first button to open an ActionSheet. Click anywhere outside of the ActionSheet to close it. Then click the second button. If nothing happens (and the app is basically frozen), this test has failed.", AutomationId = LabelId };

			Label skip = new Label { Text = "Skip this test -- this is not an iPad, so this is not relevant.", AutomationId = Skip };

			Button button = new Button { Text = "Click me first", AutomationId = Button1Id };
			button.Clicked += async (s, e) =>
			{
				string action = await DisplayActionSheet(null, null, null, Action1, "Click outside ActionSheet instead");
				System.Diagnostics.Debug.WriteLine("## " + action);
			};

			Button button2 = new Button { Text = "Click me second", AutomationId = Button2Id };
			button2.Clicked += (s, e) =>
			{
				instructions.Text = Success;
			};

			StackLayout stackLayout = new StackLayout
			{
				Children = {
					instructions,
					button,
					button2
				}
			};

			if (Device.Idiom != TargetIdiom.Tablet || Device.RuntimePlatform != Device.iOS)
				stackLayout.Children.Insert(0, skip);

			Content = stackLayout;
		}

#if UITEST && __IOS__
		[Test]
		public void Issue3049Test ()
		{
			RunningApp.WaitForElement (q => q.Marked (Button1Id));

			if (RunningApp.Query(q => q.Marked(Skip)).Length > 0)
				Assert.Pass("Test ignored, not relevant on phone");
			else
			{
				RunningApp.Tap (q => q.Marked (Button1Id));

				RunningApp.WaitForElement (q => q.Marked (Action1));

				// tap outside ActionSheet to dismiss it
				RunningApp.Tap (q => q.Marked (LabelId));

				RunningApp.WaitForElement (q => q.Marked (Button2Id));
				RunningApp.Tap (q => q.Marked (Button2Id));

				RunningApp.WaitForElement (q => q.Marked (Success));
			}
		}
#endif
	}
}