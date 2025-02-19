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
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}
