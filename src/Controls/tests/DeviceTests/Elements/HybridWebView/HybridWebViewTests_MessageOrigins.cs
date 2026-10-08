#if WINDOWS || IOS || MACCATALYST || ANDROID
#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_MessageOrigins : HybridWebViewTestsBase
{
	const string OtherOrigin = "https://example.invalid/";
	static readonly string OtherOriginDocument = $$"""
		<!DOCTYPE html>
		<html>
			<body id="otherOriginDocument">
				<script>
					function sendBridgeMessage(message) {
						if (window.chrome && window.chrome.webview) {
							window.chrome.webview.postMessage(encodeURIComponent(message));
						} else if (window.webkit && window.webkit.messageHandlers) {
							window.webkit.messageHandlers.webwindowinterop.postMessage(message);
						} else {
							return fetch("{{new Uri(HybridWebViewHandler.AppOriginUri, HybridWebViewHandler.SendMessagePath).AbsoluteUri}}", {
								method: "POST",
								headers: {
									"Content-Type": "text/plain",
									"X-Maui-Invoke-Token": "HybridWebView",
									"X-Maui-Request-Body": message
								},
								body: message
							}).catch(() => {});
						}
					}

					function sendRawMessage() {
						sendBridgeMessage("__RawMessage|Message from another origin");
					}

					function completeInvoke(taskId) {
						sendBridgeMessage(`__InvokeJavaScriptCompleted|${taskId}|"Result from another origin"`);
					}
				</script>
			</body>
		</html>
		""";

	static readonly TimeSpan OperationWaitTimeout = TimeSpan.FromSeconds(25);
	static readonly TimeSpan MessageWaitTimeout = TimeSpan.FromSeconds(5);

	[Fact]
	public Task RawMessagesFromOtherOriginsAreIgnored() =>
		RunOriginTest(async (handler, hybridWebView) =>
		{
			var messageReceived = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
			hybridWebView.RawMessageReceived += (_, args) => messageReceived.TrySetResult(args.Message);

			await LoadOtherOriginDocumentAsync(handler, hybridWebView, OtherOriginDocument);
			await AssertOtherOriginDocumentLoaded(hybridWebView);

#if ANDROID
			var bridgeAttempted = WaitForBridgeAttemptAsync(hybridWebView);
#endif
			await hybridWebView.EvaluateJavaScriptAsync("sendRawMessage()");
#if ANDROID
			await bridgeAttempted;
#endif

			Assert.False(
				await CompletesWithinAsync(messageReceived.Task, MessageWaitTimeout),
				"A raw message from another origin was delivered to the app.");
		});

	[Fact]
	public Task InvokeCompletionsFromOtherOriginsAreIgnored() =>
		RunOriginTest(async (handler, hybridWebView) =>
		{
			var taskManager = handler.GetRequiredService<IHybridWebViewTaskManager>();
			var invokeTask = taskManager.CreateTask();

			await LoadOtherOriginDocumentAsync(handler, hybridWebView, OtherOriginDocument);
			await AssertOtherOriginDocumentLoaded(hybridWebView);

#if ANDROID
			var bridgeAttempted = WaitForBridgeAttemptAsync(hybridWebView);
#endif
			await hybridWebView.EvaluateJavaScriptAsync($"completeInvoke('{invokeTask.TaskId}')");
#if ANDROID
			await bridgeAttempted;
#endif

			Assert.False(
				await CompletesWithinAsync(invokeTask.TaskCompletionSource.Task, MessageWaitTimeout),
				"An invoke task was completed by a message from another origin.");
		});

	Task RunOriginTest(Func<HybridWebViewHandler, HybridWebView, Task> test)
	{
		var hybridWebView = new HybridWebView
		{
			WidthRequest = 100,
			HeightRequest = 100,
			HybridRoot = "HybridTestRoot",
			DefaultFile = "index.html",
		};

		return RunTest(hybridWebView, test);
	}

	static async Task AssertOtherOriginDocumentLoaded(HybridWebView hybridWebView)
	{
		var loaded = await hybridWebView.EvaluateJavaScriptAsync(
			"document.getElementById('otherOriginDocument') !== null");

		Assert.Equal("true", loaded);
	}

	static async Task<bool> CompletesWithinAsync(Task task, TimeSpan timeout) =>
		await Task.WhenAny(task, Task.Delay(timeout)) == task;

	private static partial Task LoadOtherOriginDocumentAsync(
		HybridWebViewHandler handler,
		HybridWebView hybridWebView,
		string html);

#if ANDROID
	private static partial Task WaitForBridgeAttemptAsync(HybridWebView hybridWebView);
#endif
}
#endif
