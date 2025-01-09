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
		[Category(UITestCategories.Visual)]
		public void Issue24414Test()
		{
			var label = App.WaitForElement("TheLabel");
			var ex = TryVerifyScreenshot("Issue24414Test");

			for (int i = 1; i <= 3; i++)
			{
				label.Tap();
				var exception = TryVerifyScreenshot("Issue24414Test_" + i);
				ex ??= exception;
			}

			if (ex != null)
			{
				throw ex;
			}
		}

		private Exception? TryVerifyScreenshot(string fileName)
		{
			try
			{
				VerifyScreenshot(fileName);
			}
			catch(Exception ex)
			{
				return ex;
			}

			return null;
		}
	}
}
