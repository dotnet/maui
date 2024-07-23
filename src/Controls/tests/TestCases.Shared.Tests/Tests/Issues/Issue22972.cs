using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22972 : _IssuesUITest
	{
		public Issue22972(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Win platform WebView cannot be release after its parent window get close";

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewDoesNotLeak()
		{
			App.AssertMemoryTest();
		}
	}
}