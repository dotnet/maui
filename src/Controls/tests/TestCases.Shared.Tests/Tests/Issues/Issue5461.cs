using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5461 : _IssuesUITest
	{
		const string Success = "If you can see this, the test has passed";

		public Issue5461(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] ScrollView crashes when setting ScrollbarFadingEnabled to false in Custom Renderer";

		[Test]
		[Category(UITestCategories.ScrollView)]
		[Category(UITestCategories.Compatibility)]
		public void ScrollViewWithScrollbarFadingEnabledFalseDoesntCrash()
		{
			App.WaitForElement(Success);
		}
	}
}