using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27101 : _IssuesUITest
	{
		public Issue27101(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "PlatformView cannot be null here Exception in Windows";

		[Test]
		[Category(UITestCategories.Button)]
		public void NoCrashButton()
		{
			// The issue can be reproduced after a few attempts, but not always directly.
			// Iterate 10 times to perform the navigation actions tapping Buttons with Visual States.
			for (int i = 0; i < 10; i++)
			{
				App.WaitForElement("NavigateButton");
				App.Tap("NavigateButton");

				App.WaitForElement("NavigateBackButton");
				App.Tap("NavigateBackButton");
			}

			// Without errors, the test has passed.
		}
	}
}