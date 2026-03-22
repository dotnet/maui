#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29634 : _IssuesUITest
{
	public override string Issue => "iOS CV: Empty view not resizing when bounds change";

	public Issue29634(TestDevice device)
	: base(device)
	{ }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyEmptyViewResizesWhenBoundsChange()
	{
		App.WaitForElement("RunTest");
		App.Tap("RunTest");
		App.WaitForElement("SuccessLabel");
	}
}
#endif