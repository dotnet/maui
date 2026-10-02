using System;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class HybridWebViewTests : BaseTestFixture
	{
		[Fact]
		public void ExplicitInterfaceInvokeTargetPropertiesReturnNullBeforeTargetIsSet()
		{
			var hybridWebView = (IHybridWebView)new HybridWebView();

			Assert.Null(hybridWebView.InvokeJavaScriptTarget);
			Assert.Null(hybridWebView.InvokeJavaScriptType);
		}

		[Fact]
		public void InvokerGetterThrowsBeforeTargetIsSet()
		{
			var hybridWebView = new HybridWebView();

			Assert.Throws<InvalidOperationException>(() => hybridWebView.Invoker);
		}
	}
}
