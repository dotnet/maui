using Microsoft.Maui.Controls.Shapes;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class HybridWebViewFeatureTests : UITest
{
	public const string HybridWebViewFeatureMatrix = "HybridWebView Feature Matrix";


	public HybridWebViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(HybridWebViewFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_DefaultValues()
	{
		App.WaitForElement("HybridRootLabel");
		var hybridRootLabel = App.FindElement("HybridRootLabel").GetText();
		Assert.That(hybridRootLabel, Is.EqualTo("HybridWebView1"), "Hybrid Root label should be displayed correctly.");
		var hybridDefaulltFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaulltFile, Is.EqualTo("index.html"), "Default file should be index.Html.");
	}

	[Test]
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
		var hybridDefaulltFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaulltFile, Is.EqualTo("image.html"), "Default file should be index.Html.");
	}

	[Test]
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
		var hybridDefaulltFile = App.FindElement("DefaultFileLabel").GetText();
		Assert.That(hybridDefaulltFile, Is.EqualTo("index.html"), "Default file should be index.Html.");
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_EvaluateJavaScriptWithDifferentHybridRoot()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("HybridWebView1Button");
		App.Tap("HybridWebView1Button");
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		App.WaitForElement("StatusLabel");
		var result = App.FindElement("StatusLabel").GetText();
		Assert.That(result, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView1"), "JavaScript evaluation should return the correct title for HybridWebview1.");

		App.WaitForElement("HybridWebView2Button");
		App.Tap("HybridWebView2Button");
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		App.WaitForElement("StatusLabel");
		var result2 = App.FindElement("StatusLabel").GetText();
		Assert.That(result2, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView2"), "JavaScript evaluation should return the correct title for HybridWebview2.");
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebView_EvaluateJavaScriptWithDifferentDefaultFile()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("HybridWebView1Button");
		App.Tap("HybridWebView1Button");
		App.WaitForElement("ImageHtmlButton");
		App.Tap("ImageHtmlButton");
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		App.WaitForElement("StatusLabel");
		var result = App.FindElement("StatusLabel").GetText();
		Assert.That(result, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView Image Page"));

		App.WaitForElement("NavigationHtmlButton");
		App.Tap("NavigationHtmlButton");
		App.WaitForElement("EvaluateJavaScriptButton");
		App.Tap("EvaluateJavaScriptButton");
		App.WaitForElement("StatusLabel");
		var result2 = App.FindElement("StatusLabel").GetText();
		Assert.That(result2, Is.EqualTo("EvaluateJavaScriptAsync Result: HybridWebView Navigation Page"));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebViewWithShadow()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("StatusLabel", timeout: TimeSpan.FromSeconds(5));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void VerifyHybridWebViewWithIsVisibleFalse()
	{
		App.WaitForElement("ResetButton");
		App.Tap("ResetButton");
		App.WaitForElement("IsVisibleCheckBox");
		App.Tap("IsVisibleCheckBox");
		App.WaitForNoElement("HybridWebViewControl");
	}

	[Test]
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
		App.WaitForElement("StatusLabel");
		var message = App.FindElement("StatusLabel").GetText();
		Assert.That(message, Is.EqualTo("Message sent successfully. Result: Message received"), "JavaScript should receive the message sent from C#.");
	}
}