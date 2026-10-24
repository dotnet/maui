using Microsoft.Maui.Controls.Shapes;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class HybridWebViewFeatureTests : _GalleryUITest
{
	public const string HybridWebViewFeatureMatrix = "HybridWebView Feature Matrix";

	public override string GalleryPageName => HybridWebViewFeatureMatrix;


	public HybridWebViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_DefaultValues()
	{
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView1"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaultFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaultFile, Is.EqualTo("index.html"), "Default file should be index.Html.");
	}

	[Test, Order(2)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_SameHybridRootWithDifferentDefaultFile()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("HybridWebView1Button");
		App.Tap("HybridWebView1Button");
		App.WaitForElement("ImageHtmlButton");
		App.Tap("ImageHtmlButton");
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView1"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaultFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaultFile, Is.EqualTo("image.html"), "Default file should be index.Html.");
	}

	[Test, Order(3)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_SameDefaultFileWithDifferentHybridRoot()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("HybridWebView2Button");
		App.Tap("HybridWebView2Button");
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView2"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaultFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaultFile, Is.EqualTo("index.html"), "Default file should be index.Html.");
	}

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32721

	[Test, Order(4)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_EvaluateJavaScriptWithDifferentHybridRoot()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("HybridWebView1Button");
		App.Tap("HybridWebView1Button");
		Thread.Sleep(2000); // Allow time for the UI to update
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		WaitForJavaScriptResult("EvaluateJavaScriptAsync Result: HybridWebView1");
		var result = App.FindElement("StatusLabel").GetText();
		Assert.That(result, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView1"), "JavaScript evaluation should return the correct title for HybridWebview1.");

		App.WaitForElement("HybridWebView2Button");
		App.Tap("HybridWebView2Button");
		Thread.Sleep(2000); // Allow time for the UI to update
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		WaitForJavaScriptResult("EvaluateJavaScriptAsync Result: HybridWebView2");
		var result2 = App.FindElement("StatusLabel").GetText();
		Assert.That(result2, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView2"), "JavaScript evaluation should return the correct title for HybridWebview2.");
	}

	[Test, Order(5)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_EvaluateJavaScriptWithDifferentDefaultFile()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("HybridWebView1Button");
		App.Tap("HybridWebView1Button");
		App.WaitForElement("ImageHtmlButton");
		App.Tap("ImageHtmlButton");
		Thread.Sleep(2000); // Allow time for the UI to update
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		WaitForJavaScriptResult("EvaluateJavaScriptAsync Result: HybridWebView Image Page");
		var result = App.FindElement("StatusLabel").GetText();
		Assert.That(result, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView Image Page"));

		App.WaitForElement("NavigationHtmlButton");
		App.Tap("NavigationHtmlButton");
		Thread.Sleep(2000); // Allow time for the UI to update
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		WaitForJavaScriptResult("EvaluateJavaScriptAsync Result: HybridWebView Navigation Page");
		var result2 = App.FindElement("StatusLabel").GetText();
		Assert.That(result2, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView Navigation Page"));
	}

	[Test, Order(9)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebViewWithShadow()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		Thread.Sleep(2000); // Allow time for the UI to update
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(8)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebViewWithIsVisibleFalse()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("IsVisibleCheckBox");
		App.Tap("IsVisibleCheckBox");
		App.WaitForNoElement("HybridWebViewControl");
	}

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32721

	[Test, Order(7)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_SendMessageToJavaScript()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("HybridWebView2Button");
		App.Tap("HybridWebView2Button");
		App.WaitForElement("IndexHtmlButton");
		App.Tap("IndexHtmlButton");
		App.WaitForElement("SendMessageButton");
		App.Tap("SendMessageButton");
		App.WaitForTextToBePresentInElement("StatusLabel", "Message sent successfully. Result: Message received");
		var message = App.FindElement("StatusLabel").GetText();
		Assert.That(message, Is.EqualTo("Message sent successfully. Result: Message received"), "JavaScript should receive the message sent from C#.");
	}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/30575, https://github.com/dotnet/maui/issues/30605
	[Test, Order(10)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebViewWithFlowDirection()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("FlowDirectionCheckBox");
		App.Tap("FlowDirectionCheckBox");
		Thread.Sleep(2000); // Allow time for the UI to update
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	void WaitForJavaScriptResult(string expected)
	{
		// Navigation can still be pending after the previous evaluation completes.
		Assert.That(() =>
		{
			var button = App.FindElement("EvaluateJavaScriptButton");
			if (!button.IsEnabled())
				return false;

			if (App.FindElement("StatusLabel").GetText() == expected)
				return true;

			button.Click();
			return false;
		}, Is.True.After(15000, 250), $"The document did not become ready for evaluation: {expected}");
	}
}