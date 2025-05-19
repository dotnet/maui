using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20419 : _IssuesUITest
{
	public Issue20419(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Argument Exception raised when the GetStringSize method of ICanvas called with default font";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void Issue20419ArgumentException()
	{
		App.WaitForElement("descriptionLabel");
	}
}
