using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18430 : _IssuesUITest
{
	public Issue18430(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CanvasDrawingSession Exception caused on Windows";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void Issue18430ExceptionShouldNotThrown()
	{
		App.WaitForElement("Issue18430DescriptionLabel");
	}
}
