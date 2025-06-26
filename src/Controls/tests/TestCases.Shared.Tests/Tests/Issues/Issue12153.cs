using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12153 : _IssuesUITest
	{
		public Issue12153(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting FontFamily to pre-installed fonts on UWP crashes";

		[Fact]
		[Trait("Category", UITestCategories.Label)]
		[Trait("Category", UITestCategories.Compatibility)]
		public void InvalidFontDoesntCauseAppToCrash()
		{
			App.WaitForElement("Success");
		}
	}
}