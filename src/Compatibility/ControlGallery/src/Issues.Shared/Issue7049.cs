using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7049, "Null reference exception on some Android devices - Microsoft.Maui.Controls.Platform.Android.PageRenderer.Microsoft.Maui.Controls.Platform.Android.IOrderedTraversalController.UpdateTraversalOrder", PlatformAffected.Android)]
	public class Issue7049 : TestContentPage
	{
		const string ContinueButton = "ContinueButton";
		const string View1 = "View1";
		const string View2 = "View2";
		const string View3 = "View3";

		bool _flag = true;

		protected override void Init()
		{

			var button = new Button { AutomationId = ContinueButton, Text = "Continue" };
			button.Clicked += (_, __) =>
			{
				var view = _flag ?
					(View)new Entry { AutomationId = View2, Text = "Press 1 time to crash" } :
					new Label { AutomationId = View3, Text = "I'm shown, bug fixed!" };
				_flag ^= true;
				(Content as StackLayout).Children[1] = view;
			};
			Content = new StackLayout
			{
				Children =
				{
					button,
					new Label { AutomationId = View1, Text = "Press 2 times to crash" }
				}
			};
		}

#if UITEST && __ANDROID__
		[Test]
		[Description ("Test null reference in IOrderedTraversalController.UpdateTraversalOrder of Android PageRenderer")]
		public void Issue7049TestsNullRefInUpdateTraversalOrder()
		{
			RunningApp.WaitForElement(View1);
			RunningApp.Tap(ContinueButton);
			RunningApp.WaitForElement(View2);
			RunningApp.Tap(ContinueButton);
			RunningApp.WaitForElement(View3);
			RunningApp.Tap(ContinueButton);
		}
#endif
	}
}
