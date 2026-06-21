using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6387 : _IssuesUITest
{
	public Issue6387(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ArgumentException thrown when a negative value is set for the padding of a label";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelWithNegativePaddingShouldNotThrowException()
	{
		App.WaitForElement("LabelWithNegativePaddingValue");
	}
}