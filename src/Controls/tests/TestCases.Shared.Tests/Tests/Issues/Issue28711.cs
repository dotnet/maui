using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28711 : _IssuesUITest
{
	public Issue28711(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NRE occurs when using Name for SolidColorBrush";

	[Test]
	[Category(UITestCategories.Brush)]
	public void BrushWithNameDoesNotCrash()
	{
		App.WaitForElement("label");
	}
}
