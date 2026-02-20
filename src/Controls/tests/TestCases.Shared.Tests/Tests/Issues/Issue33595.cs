#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33595 : _IssuesUITest
{
	public override string Issue => "[net10] iOS 18.6 crashing on navigating to a ContentPage with Padding set and Content set to a Grid with RowDefinitions Star,Auto with ScrollView on row 0";

	public Issue33595(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyNavigationToPageWithPaddingAndScrollView()
	{
		// Tap the navigate button to push the target page
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");

		// If the page navigates successfully without crashing/freezing,
		// we should be able to find the success label and continue button
		var successLabel = App.WaitForElement("SuccessLabel");
		Assert.That(successLabel.GetText(), Is.EqualTo("Page loaded successfully"));

		var continueButton = App.WaitForElement("ContinueButton");
		Assert.That(continueButton, Is.Not.Null);
	}
}
#endif
