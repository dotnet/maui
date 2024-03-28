using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;

namespace UITests
{
	public class Github1625 : IssuesUITest
	{
		public Github1625(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Slider value is not changed for the first position change\"";

		[Test]
		[Category(UITestCategories.Slider)]
		public void SettingSliderToSpecificValueWorks()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("LabelValue");
			ClassicAssert.AreEqual("5", RunningApp.WaitForElement("LabelValue").GetText());
			RunningApp.Tap("SetTo7");
			ClassicAssert.AreEqual("7", RunningApp.WaitForElement("LabelValue").GetText());
		}
	}
}