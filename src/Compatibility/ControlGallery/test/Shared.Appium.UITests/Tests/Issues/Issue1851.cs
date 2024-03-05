using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1851 : IssuesUITest
	{
		public Issue1851(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ObservableCollection in ListView gets Index out of range when removing item";

		[Test]
		[Category(UITestCategories.ListView)]
		public void Issue1851Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("btn");
			App.Click("btn");
			App.WaitForElement("btn");
			App.Click("btn");
			App.WaitForElement("btn");
		}
	}
}
