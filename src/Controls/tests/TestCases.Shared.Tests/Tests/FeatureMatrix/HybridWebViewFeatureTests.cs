using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.WebView)]
public class HybridWebViewFeatureTests : _GalleryUITest
{
	public const string HybridWebViewFeatureMatrix = "HybridWebView Feature Matrix";

	public override string GalleryPageName => HybridWebViewFeatureMatrix;


	public HybridWebViewFeatureTests(TestDevice device)
		: base(device)
	{
	}
	[Test, Order(1)]
	public void VerifyHybridWebView_DefaultValues()
	{
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView1"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaultFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaultFile, Is.EqualTo("index.html"), "Default file should be index.Html.");
	}

	[Test, Order(2)]
	public void VerifyHybridWebView_SameHybridRootWithDifferentDefaultFile()
	{
		ResetToDefaults();
		App.WaitForElement("ImageHtmlButton");
		App.Tap("ImageHtmlButton");
		WaitForStatus("Loaded: HybridWebView Image Page");
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView1"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaultFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaultFile, Is.EqualTo("image.html"), "Default file should be image.html.");
	}

	[Test, Order(3)]
	public void VerifyHybridWebView_SameDefaultFileWithDifferentHybridRoot()
	{
		ResetToDefaults();
		App.WaitForElement("HybridWebView2Button");
		App.Tap("HybridWebView2Button");
		WaitForStatus("Loaded: HybridWebView2");
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView2"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaultFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaultFile, Is.EqualTo("index.html"), "Default file should be index.Html.");
	}

	[Test, Order(4)]
	public void VerifyHybridWebView_SameHybridRootWithNavigationDefaultFile()
	{
		ResetToDefaults();
		App.WaitForElement("NavigationHtmlButton");
		App.Tap("NavigationHtmlButton");
		WaitForStatus("Loaded: HybridWebView Navigation Page");
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView1"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaultFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaultFile, Is.EqualTo("navigation.html"), "Default file should be navigation.html.");
	}

	[Test, Order(5)]
	public void VerifyHybridWebView_SameHybridRootWithWebDefaultFile()
	{
		ResetToDefaults();
		App.WaitForElement("WebHtmlButton");
		App.Tap("WebHtmlButton");
		WaitForStatus("Loaded: Simple Web Demo");
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView1"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaultFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaultFile, Is.EqualTo("web.html"), "Default file should be web.html.");
	}

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32721

	[Test, Order(6)]
	public void VerifyHybridWebView_EvaluateJavaScriptWithDifferentHybridRoot()
	{
		ResetToDefaults();
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		WaitForStatus("EvaluateJavaScriptAsync Result: HybridWebView1");
		var result = App.FindElement("StatusLabel").GetText();
		Assert.That(result, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView1"), "JavaScript evaluation should return the correct title for HybridWebview1.");

		App.WaitForElement("HybridWebView2Button");
		App.Tap("HybridWebView2Button");
		WaitForStatus("Loaded: HybridWebView2");
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		WaitForStatus("EvaluateJavaScriptAsync Result: HybridWebView2");
		var result2 = App.FindElement("StatusLabel").GetText();
		Assert.That(result2, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView2"), "JavaScript evaluation should return the correct title for HybridWebview2.");
	}

	[Test, Order(7)]
	public void VerifyHybridWebView_EvaluateJavaScriptWithDifferentDefaultFile()
	{
		ResetToDefaults();
		App.WaitForElement("ImageHtmlButton");
		App.Tap("ImageHtmlButton");
		WaitForStatus("Loaded: HybridWebView Image Page");
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		WaitForStatus("EvaluateJavaScriptAsync Result: HybridWebView Image Page");
		var result = App.FindElement("StatusLabel").GetText();
		Assert.That(result, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView Image Page"));

		App.WaitForElement("NavigationHtmlButton");
		App.Tap("NavigationHtmlButton");
		WaitForStatus("Loaded: HybridWebView Navigation Page");
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		WaitForStatus("EvaluateJavaScriptAsync Result: HybridWebView Navigation Page");
		var result2 = App.FindElement("StatusLabel").GetText();
		Assert.That(result2, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView Navigation Page"));
	}

	[Test, Order(8)]
	public void VerifyHybridWebViewWithShadow()
	{
		ResetToDefaults();
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(9)]
	public void VerifyHybridWebViewWithIsVisibleFalse()
	{
		ResetToDefaults();
		App.WaitForElement("IsVisibleCheckBox");
		App.Tap("IsVisibleCheckBox");
		App.WaitForNoElement("HybridWebViewControl");
	}

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32721

	[Test, Order(10)]
	public void VerifyHybridWebView_SendMessageToJavaScript()
	{
		ResetToDefaults();
		App.WaitForElement("HybridWebView2Button");
		App.Tap("HybridWebView2Button");
		WaitForStatus("Loaded: HybridWebView2");
		App.WaitForElement("SendMessageButton");
		App.Tap("SendMessageButton");
		WaitForStatus("Raw message received: You said: Hello from C#");
		var message = App.FindElement("StatusLabel").GetText();
		Assert.That(message, Is.EqualTo("Raw message received: You said: Hello from C#"), "The raw message should round-trip from C# through JavaScript.");
	}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/30575, https://github.com/dotnet/maui/issues/30605
	[Test, Order(11)]
	public void VerifyHybridWebViewWithFlowDirection()
	{
		ResetToDefaults();
		App.WaitForElement("FlowDirectionCheckBox");
		App.Tap("FlowDirectionCheckBox");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	void ResetToDefaults()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		Assert.That(App.WaitForTextToBePresentInElement("HybridRootLabel", "HybridWebView1"), Is.True);
		Assert.That(App.WaitForTextToBePresentInElement("DefaultFileLabel", "index.html"), Is.True);
	}

	void WaitForStatus(string status)
	{
		Assert.That(
			App.WaitForTextToBePresentInElement("StatusLabel", status, timeout: TimeSpan.FromSeconds(10)),
			Is.True,
			$"Status should be updated to '{status}'.");
	}
}
