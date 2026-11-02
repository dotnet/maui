using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32995 : _IssuesUITest
{
	public Issue32995(TestDevice device) : base(device) { }

	public override string Issue => "TabBarDisabledColor not applied to disabled tabs on iOS";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void TabBarDisabledColorAppliedToDisabledTab()
	{
		Exception? exception = null;
		App.WaitForElement("Tab2");
		VerifyScreenshotOrSetException(ref exception,"DisabledTabWithGreenColor");
		App.Tap("EnableButton");
		App.WaitForElement("Tab2");
		VerifyScreenshotOrSetException(ref exception,"EnabledTabWithNormalColor");
		if(exception is not null)
		{
			throw exception;
		}
	}
}
