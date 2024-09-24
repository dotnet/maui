#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1436 : _IssuesUITest
	{
		public Issue1436(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Button border not drawn on Android without a BorderRadius"; 
		
		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void Issue1436Test()
		{
			App.WaitForElement("TestReady");
			App.Screenshot("I am at Issue 1436");
		}
	}
}
#endif