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

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue3884Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("entryUpdate_Spacing");
			App.Click("entryUpdate_Spacing");
			App.ClearText("entryUpdate_Spacing");
			App.EnterText("entryUpdate_Spacing", "0,500");
			App.Click("btnUpdate_Spacing");

			// If it hasn't crashed, we should still be able to find this
			App.WaitForElement("entryUpdate_Spacing");
		}
	}
}
