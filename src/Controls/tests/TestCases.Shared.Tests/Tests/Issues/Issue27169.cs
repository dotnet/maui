using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27169(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "Grid inside ScrollView should measure with infinite constraints";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollViewContentLayoutMeasuresWithInfiniteConstraints()
		{
			var measuredHeight = App.WaitForElement("StubLabel").GetText()!;
			ClassicAssert.AreEqual("200", measuredHeight);
		}
	}
}