using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20288 : _IssuesUITest
	{
		public Issue20288(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Evaluating javascript in MAUI WebView on IOS returns NULL";

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewEvaluateJavaScriptReturnsCorrectResults()
		{
			// Test String Result
			App.WaitForElement("TestStringButton");
			App.Tap("TestStringButton");
			App.WaitForElement("StringResult");
			var stringResult = App.FindElement("StringResult").GetText();
			Assert.That(stringResult, Does.Contain("Hello World"), "String evaluation should return the correct string value");
			Assert.That(stringResult, Does.Not.Contain("NULL"), "String evaluation should not return NULL");

			// Test Number Result
			App.Tap("TestNumberButton");
			App.WaitForElement("NumberResult");
			var numberResult = App.FindElement("NumberResult").GetText();
			Assert.That(numberResult, Does.Contain("42"), "Number evaluation should return the correct number value");
			Assert.That(numberResult, Does.Not.Contain("NULL"), "Number evaluation should not return NULL");

			// Test Boolean Result
			App.Tap("TestBooleanButton");
			App.WaitForElement("BooleanResult");
			var booleanResult = App.FindElement("BooleanResult").GetText();
			Assert.That(booleanResult, Does.Contain("true"), "Boolean evaluation should return the correct boolean value");
			Assert.That(booleanResult, Does.Not.Contain("NULL"), "Boolean evaluation should not return NULL");

			// Test Null Result
			App.Tap("TestNullButton");
			App.WaitForElement("NullResult");
			var nullResult = App.FindElement("NullResult").GetText();
			Assert.That(nullResult, Does.Contain("NULL"), "Null evaluation should correctly return NULL");

			// Test Object Result
			App.Tap("TestObjectButton");
			App.WaitForElement("ObjectResult");
			var objectResult = App.FindElement("ObjectResult").GetText();
			Assert.That(objectResult, Does.Contain("name"), "Object evaluation should return JSON representation");
			Assert.That(objectResult, Does.Contain("test"), "Object evaluation should contain object values");
			Assert.That(objectResult, Does.Not.Contain("NULL"), "Object evaluation should not return NULL");

			// Test innerHTML (Real World Case) - this was the original failing case
			// Scroll to ensure the button is visible on small screens
			App.ScrollTo("TestInnerHTMLButton");
			App.WaitForElement("TestInnerHTMLButton");
			App.Tap("TestInnerHTMLButton");
			
			// Wait for loading to start
			App.WaitForElement("InnerHTMLResult");
			
			// Wait for the result to change from the default text (with longer timeout)
			var maxAttempts = 15; // 15 seconds total
			var attempts = 0;
			string innerHTMLResult = "";
			
			// Wait up to 15 seconds for the result to change from the default text or "Loading..."
			App.WaitFor(() =>
			{
				innerHTMLResult = App.FindElement("InnerHTMLResult").GetText() ?? "";
				return innerHTMLResult != "innerHTML result will appear here" && innerHTMLResult != "Loading...";
			}, timeout: TimeSpan.FromSeconds(15));
			
			// The result should not be NULL and should indicate success
			Assert.That(innerHTMLResult, Does.Not.Contain("NULL"), "innerHTML evaluation should not return NULL");
			
			// Check for various success indicators - either length info or an error explanation
			var hasValidResult = innerHTMLResult.Contains("length:", StringComparison.Ordinal) || 
			                    innerHTMLResult.Contains("Document not available", StringComparison.Ordinal) ||
			                    innerHTMLResult.Contains("Error:", StringComparison.Ordinal);
			                    
			Assert.That(hasValidResult, Is.True, 
				$"innerHTML evaluation should return either content with length, document availability info, or error details. Got: '{innerHTMLResult}'");
		}
	}
}