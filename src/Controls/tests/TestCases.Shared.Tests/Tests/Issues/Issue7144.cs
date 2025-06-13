using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7144 : _IssuesUITest
{
	public Issue7144(TestDevice device) : base(device)
	{
	}

	public override string Issue => "IndicatorView using templated icons not working";

	[Fact]
	[Category(UITestCategories.IndicatorView)]
	public void IndicatorViewWithTemplatedIcon()
	{
		App.WaitForElement("descriptionLabel");
		VerifyScreenshot();
	}
}
