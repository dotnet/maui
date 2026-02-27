#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Select items traces are preserved Issue Link - https://github.com/dotnet/maui/issues/26187
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10025 : _IssuesUITest
{
	public Issue10025(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Assigning null to the SelectedItem of the CollectionView in the SelectionChanged event does not clear the selection as expected";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectedItemClearsOnNullAssignment()
	{
		App.WaitForElement("Item1");
		App.Tap("Item1");
		App.WaitForElement("DescriptionLabel");
		App.Tap("DescriptionLabel");
		VerifyScreenshot();
	}
}
#endif