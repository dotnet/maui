using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue12374 : IssuesUITest
	{
		public Issue12374(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] iOS XF 5.0-pre1 crash with CollectionView when using EmptyView";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnIOS]
		public void Issue12374Test()
		{
			RunningApp.WaitForElement("TestReady");
			RunningApp.Tap("RemoveItems");
			RunningApp.Tap("AddItems");
			RunningApp.Tap("RemoveItems");
			RunningApp.Screenshot("CollectionViewWithEmptyView");
		}
	}
}