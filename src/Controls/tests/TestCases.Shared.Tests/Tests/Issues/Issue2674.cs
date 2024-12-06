using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2674 : _IssuesUITest
	{
		public Issue2674(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Exception occurs when giving null values in picker itemsource collection";

		[Test]
		[Category(UITestCategories.Picker)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void Issue2674Test()
		{
			App.Screenshot("I am at Issue2674");
			App.WaitForElement("picker");
		}
	}
}