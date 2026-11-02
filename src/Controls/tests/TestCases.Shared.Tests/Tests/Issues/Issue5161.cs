using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5161 : _IssuesUITest
{
	public Issue5161(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ShellContent IsEnabledProperty does not work";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void CheckIsEnabled()
	{
		App.WaitForElement("ThirdTab");
		App.Tap("ThirdTab");
		App.WaitForElement("ThirdPageLabel");
		App.Tap("SecondTab");
		App.WaitForNoElement("SecondPageLabel");
		App.Tap("EnableSecondTab");
		App.Tap("SecondTab");
		App.WaitForElement("SecondPageLabel");
	}
}
