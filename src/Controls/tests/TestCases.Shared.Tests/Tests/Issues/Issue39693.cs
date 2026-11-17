#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39693 : _IssuesUITest
{
	public Issue39693(TestDevice device) : base(device) { }

	public override string Issue => "OrientationStateTriggers are broken with iOS 27";

	[Test]
	[Category(UITestCategories.Triggers)]
	public void OrientationStateTriggersUpdateColorAndGridDefinitions()
	{
		try
		{
			App.SetOrientationPortrait();
			AssertStatus("Green; rows=3; columns=1");
			App.SetOrientationLandscape();
			AssertStatus("Black; rows=2; columns=2");
			App.SetOrientationPortrait();
			AssertStatus("Green; rows=3; columns=1");
		}
		finally
		{
			App.SetOrientationPortrait();
		}
	}

	void AssertStatus(string expected)
	{
		Assert.That(App.WaitForTextToBePresentInElement("OrientationStateStatus", expected), Is.True);
		Assert.That(App.FindElement("OrientationStateStatus").GetText(), Is.EqualTo(expected));
	}
}
#endif
