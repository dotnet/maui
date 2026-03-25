using Microsoft.Maui.Handlers;
using Xunit;

#if IOS
using Foundation;
using Microsoft.Maui.Platform;
#endif

namespace Microsoft.Maui.UnitTests
{
#if IOS
	public class WebViewExtensionsIOSTests
	{
		[Fact]
		public void HandleWKWebViewResult_NSNull_ReturnsNullString()
		{
			var result = WebViewExtensions.HandleWKWebViewResult(NSNull.Null);
			Assert.Equal("null", result);
		}

		[Fact]
		public void HandleWKWebViewResult_Null_ReturnsNullString()
		{
			var result = WebViewExtensions.HandleWKWebViewResult(null);
			Assert.Equal("null", result);
		}

		[Fact]
		public void HandleWKWebViewResult_NSString_ReturnsString()
		{
			var nsString = new NSString("Hello World");
			var result = WebViewExtensions.HandleWKWebViewResult(nsString);
			Assert.Equal("Hello World", result);
		}

		[Fact]
		public void HandleWKWebViewResult_NSNumber_ReturnsNumberString()
		{
			var nsNumber = NSNumber.FromInt32(42);
			var result = WebViewExtensions.HandleWKWebViewResult(nsNumber);
			Assert.Equal("42", result);
		}

		[Fact]
		public void HandleWKWebViewResult_NSNumberBoolean_ReturnsBooleanString()
		{
			var nsNumber = NSNumber.FromBoolean(true);
			var result = WebViewExtensions.HandleWKWebViewResult(nsNumber);
			Assert.Equal("True", result);
		}

		[Fact]
		public void HandleWKWebViewResult_NSDictionary_ReturnsJSONString()
		{
			var dict = new NSMutableDictionary();
			dict.SetValueForKey(new NSString("value"), new NSString("key"));
			var result = WebViewExtensions.HandleWKWebViewResult(dict);
			Assert.Contains("key", result);
			Assert.Contains("value", result);
		}
	}
#endif
}