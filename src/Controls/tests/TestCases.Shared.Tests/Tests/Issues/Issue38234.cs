#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38234 : _IssuesUITest
{
	public override string Issue => "Android Material 3 Shell TabBar selection color does not update with AppTheme";

	public Issue38234(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void ShellTabBarSelectionColorUpdatesAfterThemeChange()
	{
		App.WaitForElement("UseNativeSelectionColorButton");
		App.Tap("UseNativeSelectionColorButton");
		VerifyThemeTransitions();
	}

	[Test]
	[Category(UITestCategories.Material3)]
	public void ShellTabBarExplicitSelectionColorSurvivesThemeChange()
	{
		App.WaitForElement("UseExplicitSelectionColorButton");
		App.Tap("UseExplicitSelectionColorButton");
		VerifyThemeTransitions();
	}

	void VerifyThemeTransitions()
	{
		App.WaitForElement("SwitchToDarkThemeButton");
		var lightColor = ReadIndicatorColor();

		App.Tap("SwitchToDarkThemeButton");
		App.WaitForElement("Dark theme active");
		Assert.That(ReadIndicatorColor(), Is.Not.EqualTo(lightColor));

		App.Tap("SwitchToDarkThemeButton");
		App.WaitForElement("Light theme active");
		Assert.That(ReadIndicatorColor(), Is.EqualTo(lightColor));
	}

	string ReadIndicatorColor()
	{
		string? color = null;
		string? expectedColor = null;
		Assert.That(() =>
		{
			App.Tap("ReadIndicatorColorButton");
			color = App.WaitForElement("IndicatorColorLabel").GetText();
			expectedColor = App.WaitForElement("ExpectedIndicatorColorLabel").GetText();
			return expectedColor is { Length: 17 } && expectedColor[8] == '/' &&
				expectedColor[..8].All(Uri.IsHexDigit) && expectedColor[9..].All(Uri.IsHexDigit) &&
				color == expectedColor;
		}, Is.True.After(5000, 100),
			() => $"Expected native Material3 indicator {expectedColor}; actual {color}.");
		return (color ?? throw new InvalidOperationException("Native selection indicator color was not reported."))[..8];
	}
}

public class Issue38234NativeTabbedPage : Issue38234
{
	public override string Issue => "Android Material 3 TabbedPage native selection colors follow AppTheme";

	public Issue38234NativeTabbedPage(TestDevice device) : base(device)
	{
	}
}
#endif