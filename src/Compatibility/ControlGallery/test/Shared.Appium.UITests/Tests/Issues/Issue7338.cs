using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue7338 : IssuesUITest
	{
		const string Success = "success";

		public Issue7338(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView crash if source is empty in XF 4.2.0.709249";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue3273Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);
			
			// If the instructions are visible at all, then this has succeeded
			App.WaitForElement(Success);
		}
	}
}