using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue9929 : IssuesUITest
	{
		public Issue9929(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] NSInternalInconsistencyException when trying to run XamarinTV on iOS";

		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnIOS]
		public void Issue3884Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("entryUpdate_Spacing");
			RunningApp.Tap("entryUpdate_Spacing");
			RunningApp.ClearText("entryUpdate_Spacing");
			RunningApp.EnterText("entryUpdate_Spacing", "0,500");
			RunningApp.Tap("btnUpdate_Spacing");

			// If it hasn't crashed, we should still be able to find this
			RunningApp.WaitForElement("entryUpdate_Spacing");
		}
	}
}
