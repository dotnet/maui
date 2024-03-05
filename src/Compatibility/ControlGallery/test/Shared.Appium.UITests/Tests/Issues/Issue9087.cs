using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue9087 : IssuesUITest
	{
		const string Success = "Success";

		public Issue9087(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] Scrollview with null content crashes on UWP";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void BindablePropertiesAvailableAtOnElementChanged()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			App.WaitForNoElement(Success);
		}
	}
}
