#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29862 : _IssuesUITest
{
	public Issue29862(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[Android] ScrollView locks when paired with Horizontal scrolling";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewShouldNotLock()
	{
		App.WaitForElement("LabelInHorizontalScrollView1");
		for (int i = 0; i < 2; i++)
		{
			App.ScrollRight("LabelInHorizontalScrollView1");
			App.ScrollDown("LabelInVerticalScrollView1");
		}
		App.WaitForElement("LabelInVerticalScrollView2");
	}
}
#endif