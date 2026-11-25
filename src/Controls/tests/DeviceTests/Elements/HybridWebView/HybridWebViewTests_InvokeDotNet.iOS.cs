#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class HybridWebViewTests_InvokeDotNet
{
	[Fact]
	public Task RoutedDocumentCanInvokeDotNet() =>
		RunTest("invokedotnettests.html", async view =>
		{
			var target = new InvocationTarget();
			view.SetInvokeJavaScriptTarget(target);

			Assert.Equal("/", await view.EvaluateJavaScriptAsync("window.location.pathname"));
			await view.EvaluateJavaScriptAsync("history.pushState({}, '', '/home')");
			Assert.Equal("/home", await view.EvaluateJavaScriptAsync("window.location.pathname"));

			await view.EvaluateJavaScriptAsync(
				"window.issue33556State = 'pending'; " +
				"window.HybridWebView.InvokeDotNet('MarkInvoked')" +
				".then(() => window.issue33556State = 'completed')" +
				".catch(error => window.issue33556State = `rejected: ${error}`); 'started'");

			string? state = null;
			Func<Task<bool>> invocationFinished = async () =>
			{
				state = await view.EvaluateJavaScriptAsync("window.issue33556State");
				return state == "completed" || (state != null && state.StartsWith("rejected: ", StringComparison.Ordinal));
			};

			await invocationFinished.AssertEventuallyAsync(timeout: 5000, message: "The routed document invocation did not finish.");

			Assert.True(target.WasCalled, $"Routed document invocation did not reach .NET; state: {state}");
			Assert.Equal("completed", state);
		});

	sealed class InvocationTarget
	{
		public bool WasCalled { get; private set; }

		public void MarkInvoked() => WasCalled = true;
	}
}
