using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Object Disposed Exception in ScrollViewContainer", PlatformAffected.Android)]
	public class ScrollViewObjectDisposed : TestContentPage
	{
		const string Instructions = "Tap the button. If the app does not crash and the red label displays \"Success\", this test has passed.";
		const string Success = "Success";
		const string TestButtonId = "TestButtonId";

		Label _status = new Label() { Text = "Test is running...", BackgroundColor = Colors.Red, TextColor = Colors.White };

		ScrollView _scroll = new ScrollView();

		protected override void Init()
		{
			_scroll.Content = _status;

			InitTest();
		}

		void InitTest()
		{

			Button nextButton = new Button { Text = "Next", AutomationId = TestButtonId };
			nextButton.Clicked += NextButton_Clicked;

			StackLayout stack = new StackLayout
			{
				Children = { new Label { Text = Instructions }, _scroll, nextButton }
			};

			Content = stack;
		}

		void NextButton_Clicked(object sender, EventArgs e)
		{
			_status.Text = "";

			InitTest();

			_status.Text = Success;
		}

#if UITEST
		[Compatibility.UITests.MovedToAppium]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void ScrollViewObjectDisposedTest ()
		{
			RunningApp.Tap(q => q.Marked(TestButtonId));
			RunningApp.WaitForElement(q => q.Marked(Success));
		}
#endif
	}
}