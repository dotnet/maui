using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Issue5239 : IssuesUITest
	{
		public Issue5239(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Top Padding not working on iOS when it is set alone";

		[Test]
		[Category(UITestCategories.Layout)]
		public void PaddingEqualToSafeAreaWorks()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			var somePadding = App.WaitForElement("Hello").GetRect();
			ClassicAssert.AreEqual(20f, somePadding.Y);
		}
	}
}