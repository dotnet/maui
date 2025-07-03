using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21034 : _IssuesUITest
	{
		public Issue21034(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Child Span does not inherit TextColor or FontAttributes from parent Label";

		[Test]
		[Category(UITestCategories.Label)]
		public void SpanShouldInheritLabelsProperties()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}
