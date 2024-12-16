#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20922 : _IssuesUITest
	{
		public Issue20922(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Shadow Doesn't Work on Grid in scroll view on Android";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ShadowShouldWork()
		{
			App.WaitForElement("Grid");
			VerifyScreenshot();
		}
	}
}
#endif