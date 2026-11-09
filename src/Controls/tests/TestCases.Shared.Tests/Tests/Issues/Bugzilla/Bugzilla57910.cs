using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla57910 : _IssuesUITest
	{
		const string ButtonId = "btnPush";
		const string Button2Id = "btnPop";

		public Bugzilla57910(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ObjectDisposedException in Microsoft.Maui.Controls.Platform.Android.Renderers.ProgressBarRenderer";

		// Crash after navigation
		[Test]
		[Category(UITestCategories.ProgressBar)]
		public void Bugzilla57910Test()
		{
			for (int i = 0; i < 10; i++)
			{
				App.WaitForElement(ButtonId, timeout: TimeSpan.FromSeconds(45));
				TapNavigationButton(ButtonId);
				App.WaitForElement(Button2Id, timeout: TimeSpan.FromSeconds(45));
				TapNavigationButton(Button2Id);
			}

			App.WaitForElement(ButtonId, timeout: TimeSpan.FromSeconds(45));
		}

		void TapNavigationButton(string automationId)
		{
			if (App is ICatalystApp)
			{
				// The fixture intentionally updates during navigation; don't wait for XCTest idle.
				var rect = App.FindElement(automationId).GetRect();
				App.TapCoordinates(rect.CenterX(), rect.CenterY());
			}
			else
			{
				App.Tap(automationId);
			}
		}
	}
}