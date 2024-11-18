#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21437 : _IssuesUITest
{
	public override string Issue => "Removing TapGestureRecognizer with at least 2 taps causes Exception";

	public Issue21437(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Gestures)]
	public void ExceptionShouldNotBeThrown()
	{
		_ = App.WaitForElement("Item2");
		App.DoubleClick("Item2");

		//The test passes if no exception is thrown
	}
}
#endif