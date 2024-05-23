using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue10182 : _IssuesUITest
	{
		public Issue10182(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Exception Ancestor must be provided for all pushes except first";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid("MultiWindowService not implemented.")]
		[FailsOnIOS]
		public void AppDoesntCrashWhenResettingPage()
		{
			App.WaitForElement("Success");
		}
	}
}