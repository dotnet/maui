using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20439 : _IssuesUITest
	{
		public override string Issue => "[iOS] Double dash in Entry or Editor crashes the app";

		public Issue20439(TestDevice device) : base(device)
		{
		}

		[Test]
		public void ErrorShouldNotBeThrown()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });

			try
			{
				_ = App.WaitForElement("GoToTest");
				App.Tap("GoToTest");
				
				_ = App.WaitForElement("entry");
				App.Tap("entry");
				App.Tap("button");

				// The test passes if no crash is observed
				App.FindElement("editor");
			}
			finally
			{
				Reset();
			}
		}
	}
}
