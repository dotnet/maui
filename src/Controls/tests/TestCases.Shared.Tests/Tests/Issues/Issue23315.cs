using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23315(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "LoadFile in src/Core/src/Platform/iOS/MauiWKWebView.cs ignore directories";

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewCanLoadFileFromSubdirectory()
		{
			// The HostApp loads `foo/bar/baz/test.html` whose <title> is "Nested Subdirectory Test File".
			// When the bug is present on iOS/MacCatalyst, LoadFile strips the directory part
			// and tries to load only `test.html`, so the navigation fails and the label
			// never reports the expected title.
			var statusLabel = App.WaitForElement("StatusLabel", timeout: TimeSpan.FromSeconds(10));
			var text = statusLabel.GetText();
			Assert.That(text, Is.EqualTo("Success"), $"Expected to load the file from the subdirectory, but got '{text}' instead.");
		}
	}
}
