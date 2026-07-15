using System.Globalization;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26924 : _IssuesUITest
{
	public Issue26924(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Font Size of span Element Not Rendering Correctly in Mobile Mode in .NET MAUI Blazor";

	[Test]
	[Category(UITestCategories.WebView)]
	public void SmallFontSizeSpanIsNotClampedByMinimumFontSize()
	{
		// The result label is updated to "WebView loaded" once the WebView finishes loading
		// the HTML, so wait for that before tapping the button to avoid racing navigation.
		App.WaitForTextToBePresentInElement("ComputedFontSizeLabel", "WebView loaded");
		App.Tap("CheckFontSizeButton");
		var computedFontSizeText = App.WaitForElement("ComputedFontSizeLabel").GetText();
		Assert.That(computedFontSizeText, Is.Not.Null);
		var computedFontSize = float.Parse(computedFontSizeText!.TrimEnd('p', 'x'), CultureInfo.InvariantCulture);
		Assert.That(computedFontSize, Is.LessThan(8f));
	}
}

