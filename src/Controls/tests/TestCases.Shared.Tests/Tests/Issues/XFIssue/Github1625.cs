using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Github1625 : _IssuesUITest
	{
		public Github1625(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Slider value is not changed for the first position change";

		[Test]
		[Category(UITestCategories.Slider)]
		public void SettingSliderToSpecificValueWorks()
		{
			App.WaitForElement("LabelValue");
			ClassicAssert.AreEqual("5", App.WaitForElement("LabelValue").GetText());
			App.Tap("SetTo7");
			ClassicAssert.AreEqual("7", App.WaitForElement("LabelValue").GetText());
		}
	}
}