#if MACCATALYST || WINDOWS
// TitleBar is only available on Mac Catalyst and Windows. https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/titlebar?view=net-maui-9.0

using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24831 : _IssuesUITest
{
	public Issue24831(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "The BindingContext of the Window TitleBar is not being passed on to its child content";

	[Test]
	[Category(UITestCategories.Window)]
	public void TitleBarShouldPropagateBindingContext()
	{
		App.WaitForElement("descriptionLabel");
		VerifyScreenshot(includeTitleBar: true);
	}
}
#endif