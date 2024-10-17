using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue3667 : _IssuesUITest
{
	public Issue3667(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Enhancement] Add text-transforms to Label";

	// [Test]
	// [Category(UITestCategories.Label)]
	// [FailsOnIOS]
	// public void Issue3667Tests ()
	// {
	// 	RunningApp.WaitForElement(query => query.Text(text));
	// 	RunningApp.Tap("Change TextTransform");
	// 	RunningApp.WaitForElement(query => query.Text(text));
	// 	RunningApp.Tap("Change TextTransform");
	// 	RunningApp.WaitForElement(query => query.Text(text.ToLowerInvariant()));
	// 	RunningApp.Tap("Change TextTransform");
	// 	RunningApp.WaitForElement(query => query.Text(text.ToUpperInvariant()));
	// 	RunningApp.Tap("Change TextTransform");
	// 	RunningApp.WaitForElement(query => query.Text(text));
	// }
}