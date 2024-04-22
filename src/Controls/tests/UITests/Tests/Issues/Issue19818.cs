using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue19818 : _IssuesUITest
	{
		public Issue19818(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[MAUI] Stuck when entering/exiting G6 page";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue19818Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("TestCollectionView");
			App.ScrollDown("TestCollectionView", ScrollStrategy.Gesture);
			App.WaitForNoElement("3");
		}
	}
}