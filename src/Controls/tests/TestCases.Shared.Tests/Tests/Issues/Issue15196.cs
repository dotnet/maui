using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue15196 : _IssuesUITest
	{
		public override string Issue => "Nested Entry View In A Frame Causes Crash";

		public Issue15196(TestDevice testDevice) : base(testDevice) { }

		[Test, Retry(2)]
		[Category(UITestCategories.Entry)]
		public void NestedEntryViewInFrameShouldNotCrash()
		{
			App.WaitForElement("RemoveButton");
			App.Click("RemoveButton");
		}
	}
}