#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23934 : _IssuesUITest
{
	public override string Issue => "RelativeLayout content disappears when it has a border with a border stroke";

	public Issue23934(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Border)]
	public void RelativeLayoutContentShouldBeAppeared()
	{
		App.WaitForElement("LabelControl");

		VerifyScreenshot();
	}

}
#endif