#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35788 : _IssuesUITest
{
	public Issue35788(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android] WebView CanGoBack returns true unexpectedly on first page due to spurious about:blank history entry";

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewCanGoBackShouldBeFalseOnFirstPage()
	{
		App.WaitForElement("Issue35788NavigateButton");
		App.Tap("Issue35788NavigateButton");

		App.WaitForTextToBePresentInElement("Issue35788StatusLabel", "CanGoBack=");

		var statusText = App.FindElement("Issue35788StatusLabel").GetText();

		Assert.That(statusText, Is.EqualTo("CanGoBack=False"),
			"WebView.CanGoBack should be false on the first navigated page. " +
			"If true, the about:blank layout entry was not cleared from the native history stack.");
	}
}
#endif
