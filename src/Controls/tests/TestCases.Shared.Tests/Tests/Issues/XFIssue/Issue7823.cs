using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7823_XF : _IssuesUITest
{
	public Issue7823_XF(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Frame corner radius.";

	//[Test]
	//[Category(UITestCategories.Frame)]
	//[FailsOnAndroid]
	//public void Issue7823TestIsClippedIssue()
	//{
	//	RunningApp.WaitForElement(RootFrame);
	//	AssertIsClipped(true);
	//	RunningApp.Tap(SetClipBounds);
	//	AssertIsClipped(false);
	//}

	//void AssertIsClipped(bool expected)
	//{
	//	if (RunningApp.IsApiHigherThan(21))
	//	{
	//		var cliptoOutlineValue = RunningApp.InvokeFromElement<bool>(SecondaryFrame, GetClipToOutline)[0];
	//		Assert.AreEqual(expected, cliptoOutlineValue);
	//	}
	//	else if (RunningApp.IsApiHigherThan(19))
	//	{
	//		var clipBounds = RunningApp.InvokeFromElement<object>(SecondaryFrame, GetClipBounds)[0];
	//		if (expected)
	//			Assert.IsNotNull(clipBounds);
	//		else
	//			Assert.IsNull(clipBounds);
	//	}
	//	else
	//	{
	//		var clipChildrenValue = RunningApp.InvokeFromElement<bool>(SecondaryFrame, GetClipChildren)[0];
	//		Assert.AreEqual(expected, clipChildrenValue);
	//	}
	//}
}