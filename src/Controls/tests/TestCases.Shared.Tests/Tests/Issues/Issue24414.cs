using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24414 : _IssuesUITest
	{
		public Issue24414(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Shadows not rendering as expected on Android and iOS";

		[Test]
		[Category(UITestCategories.Shadow)]
		public void Issue24414Test()
		{
			App.WaitForElement("TheLabel");

			Exception? exception = null;
			VerifyScreenshotOrSetException(ref exception, "Issue24414Test");

			for (int i = 1; i <= 5; i++)
			{
				App.WaitForElement("TheLabel").Tap();
				VerifyScreenshotOrSetException(ref exception, "Issue24414Test_" + i);
			}

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}