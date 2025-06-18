using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25836 : _IssuesUITest
	{
		public Issue25836(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Span with tail truncation and paragraph breaks with exception";

		[Test]
		[Category(UITestCategories.Label)]
		public void ExceptionShouldNotBeThrown()
		{
			App.WaitForElement("Label");
		}
	}
}