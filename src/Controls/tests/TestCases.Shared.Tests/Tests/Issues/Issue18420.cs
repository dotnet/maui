using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18420 : _IssuesUITest
{
	public Issue18420(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] ViewExtensions RotateYTo and RotateXTo with length 0 crashes on Windows";

	[Test]
	[Category(UITestCategories.ViewBaseTests)]
	public void ApplyingRotationWithZeroDurationShouldNotCrash()
	{
		App.WaitForElement("RotateButton");
		App.Tap("RotateButton");
		App.Tap("RotateButton");
		App.Tap("RotateButton");
		App.WaitForElement("RotateButton");
	}
}