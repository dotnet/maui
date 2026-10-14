using System;
using System.Threading;
using System.Threading.Tasks;
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

		[Fact]
		public void BinaryInvocationIsIndependentAndExplicitlyOptedIn()
		{
			var view = new HybridWebView();
			Assert.Null(((IHybridWebViewBinary)view).BinaryInvoker);
			var invoker = new BinaryInvoker();
			view.BinaryInvoker = invoker;
			Assert.Same(invoker, ((IHybridWebViewBinary)view).BinaryInvoker);
			Assert.Throws<InvalidOperationException>(() => view.Invoker);
			view.BinaryInvoker = null;
			Assert.Null(((IHybridWebViewBinary)view).BinaryInvoker);
		}

		sealed class BinaryInvoker : HybridWebViewBinaryInvoker
		{
			public override Task<byte[]> InvokeAsync(HybridWebViewBinaryRequest request, CancellationToken cancellationToken) =>
				Task.FromResult(request.Payload);
		}
	}
}
