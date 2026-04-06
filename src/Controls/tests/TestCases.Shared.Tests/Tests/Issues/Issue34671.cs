#if WINDOWS || ANDROID // Existing PR for iOS : https://github.com/dotnet/maui/pull/34672
using System.Globalization;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34671 : _IssuesUITest
{
	public override string Issue => "[Windows] ScrollView offsets do not preserve when Orientation changes to Neither";

	public Issue34671(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void Issue34671ScrollPositionIsPreservedWhenOrientationChangesToNeither()
	{
		App.WaitForElement("ScrollToReproOffsetButton");
		App.Tap("ScrollToReproOffsetButton");
		App.WaitForTextToBePresentInElement("LastActionLabel", "Scrolled to approx");
		App.WaitForElement("SetNeitherButton");
		App.Tap("SetNeitherButton");

		var offsetText = App.WaitForElement("OffsetLabel").GetText()
			?? throw new InvalidOperationException("OffsetLabel text was null.");
		var offsetParts = offsetText.Split('|', StringSplitOptions.TrimEntries);
		var scrollXText = offsetParts[0]
			.Replace("ScrollX:", string.Empty, StringComparison.Ordinal)
			.Trim();
		var scrollYText = offsetParts[1]
			.Replace("ScrollY:", string.Empty, StringComparison.Ordinal)
			.Trim();
		var scrollX = double.Parse(scrollXText, CultureInfo.InvariantCulture);
		var scrollY = double.Parse(scrollYText, CultureInfo.InvariantCulture);

		Assert.That(scrollX, Is.GreaterThan(0d), "ScrollX should remain non-zero after setting orientation to Neither.");
		Assert.That(scrollY, Is.GreaterThan(0d), "ScrollY should remain non-zero after setting orientation to Neither.");
	}
}
#endif
