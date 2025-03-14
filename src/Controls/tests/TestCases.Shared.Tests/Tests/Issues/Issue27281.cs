using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    internal class Issue27281 : _IssuesUITest
	{
		public Issue27281(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Application freezes when Background property is set to a DynamicResource of type OnPlatform Color";

		[Test]
		[Category(UITestCategories.Brush)]
		public void UpdateDynamicResourceOnPlatformColor()
		{
			App.WaitForElement("WaitForStubControl");
			App.Tap("WaitForStubControl");
			App.Tap("TestButton");
			var result = App.WaitForElement("ResultLabel").GetText();

			Assert.That(result, Is.EqualTo("Passed"));
		}
	}
}
