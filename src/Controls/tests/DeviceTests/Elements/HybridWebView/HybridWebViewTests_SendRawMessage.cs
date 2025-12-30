#nullable enable
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

[Category(TestCategory.HybridWebView)]
#if WINDOWS
[Collection(WebViewsCollection)]
#endif
public partial class HybridWebViewTests_SendRawMessage : HybridWebViewTestsBase
{
	[Fact]
	public Task LoadsHtmlAndSendReceiveRawMessage() =>
		RunTest(async (hybridWebView) =>
		{
			var lastRawMessage = "";

			hybridWebView.RawMessageReceived += (s, e) =>
			{
				lastRawMessage = e.Message;
			};

			const string TestRawMessage = "Hybrid\"\"'' {Test} with chars!";
			hybridWebView.SendRawMessage(TestRawMessage);

			var passed = false;

			for (var i = 0; i < 10; i++)
			{
				if (lastRawMessage == "You said: " + TestRawMessage)
				{
					passed = true;
					break;
				}

				await Task.Delay(1000);
			}

			Assert.True(passed, $"Waited for raw message response but it never arrived or didn't match (last message: {lastRawMessage})");
		});
}
