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
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void Issue12374Test()
		{
			App.WaitForElement("TestReady");
			App.Tap("RemoveItems");
			App.Tap("AddItems");
			App.Tap("RemoveItems");
			App.Screenshot("CollectionViewWithEmptyView");
		}
	}
}