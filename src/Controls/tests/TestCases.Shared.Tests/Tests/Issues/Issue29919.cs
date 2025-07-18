using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29919 : _IssuesUITest
{
	public Issue29919(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "StackLayout Throws Exception on Windows When Orientation Is Set with HeightRequest of 0, Padding, and Opposing Alignment";

	[Test]
	[Category(UITestCategories.Layout)]
	public void StackLayoutWindowsCrashWithZeroHeight()
	{
		App.WaitForElement("29919DescriptionLabel");
	}
}