#if WINDOWS // Issue only happens on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18172 : _IssuesUITest
{
	public Issue18172(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Shadows not drawing/updating correctly in Windows & cover entire screen";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public async Task Issue18172Test()
	{
		await Task.Delay(500);

		VerifyScreenshot();
	}
}
#endif