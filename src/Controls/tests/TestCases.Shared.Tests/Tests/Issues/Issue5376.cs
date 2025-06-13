using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue5376 : _IssuesUITest
	{
		public Issue5376(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Call unfocus entry crashes app";

		[Fact]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Compatibility)]
		public void Issue5376Test()
		{
			App.WaitForElement("Success");
		}
	}
}
