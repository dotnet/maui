#if TEST_FAILS_ON_WINDOWS // EmptyView is not accessible through the test framework on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12374 : _IssuesUITest
	{
		public Issue12374(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] iOS XF 5.0-pre1 crash with CollectionView when using EmptyView";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue12374Test()
		{
			App.WaitForElement("TestReady");
			App.WaitForElement("RemoveItems");
			App.Tap("RemoveItems");
			App.WaitForElement("AddItems");
			App.Tap("AddItems");
			App.WaitForElement("RemoveItems");
			App.Tap("RemoveItems");
			App.WaitForElement("Empty View");
		}
	}
}
#endif