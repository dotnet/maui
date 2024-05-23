using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1538 : _IssuesUITest
	{
		public Issue1538(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash measuring empty ScrollView"; 
		
		[Test]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOS]
		public void MeasuringEmptyScrollViewDoesNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			Task.Delay(1000).Wait();
			App.WaitForElement("Foo");
		}
	}
}