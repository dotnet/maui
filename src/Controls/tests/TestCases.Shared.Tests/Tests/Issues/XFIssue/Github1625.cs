using Xunit;
using Xunit;
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

		[Fact]
		[Trait("Category", UITestCategories.Slider)]
		public void SettingSliderToSpecificValueWorks()
		{
			App.WaitForElement("LabelValue");
			Assert.Equal("5", App.WaitForElement("LabelValue").GetText();
			App.Tap("SetTo7");
			Assert.Equal("7", App.WaitForElement("LabelValue").GetText();
		}
	}
}