using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2674 : _IssuesUITest
	{
		public Issue2674(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Exception occurs when giving null values in picker itemsource collection";

		[Fact]
		[Trait("Category", UITestCategories.Picker)]
		public void Issue2674Test()
		{
			App.WaitForElement("picker");
		}
	}
}