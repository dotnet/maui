using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19496 : _IssuesUITest
	{
		public override string Issue => "The Shell item text disappears when it is updated dynamically at runtime";

		public Issue19496(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.Shell)]
		public void FlyoutItemTextShouldDisplayProperly()
		{
			App.WaitForElement("button");
			App.Tap("button");
			App.Tap("button");
			App.Tap("button");
			App.Tap("button");
			VerifyScreenshot();
		}
	}
}
