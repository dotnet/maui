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

	// Regression: ensure raw messages containing characters that cannot appear in HTTP header
	// values (CR/LF/NUL) and characters that vary in header byte-set handling (non-ASCII unicode,
	// the '%' encoding sentinel) survive the Android fetch transport. The JS side URL-encodes
	// raw messages and HybridWebViewHelper.ProcessRawMessage decodes them.
	[Theory]
	[InlineData("with\nnewline")]
	[InlineData("with\r\ncarriage")]
	[InlineData("with\0nul")]
	[InlineData("100% complete")]
	[InlineData("café with é and emoji 😀")]
	public Task SendRawMessageRoundTripsSpecialCharacters(string testMessage) =>
		RunTest(async (hybridWebView) =>
		{
			var lastRawMessage = "";

			hybridWebView.RawMessageReceived += (s, e) =>
			{
				lastRawMessage = e.Message;
			};

			hybridWebView.SendRawMessage(testMessage);

			var expected = "You said: " + testMessage;
			var passed = false;

			for (var i = 0; i < 10; i++)
			{
				if (lastRawMessage == expected)
				{
					passed = true;
					break;
				}

				await Task.Delay(1000);
			}

			Assert.True(passed, $"Raw message did not round-trip. Expected: [{expected}] Got: [{lastRawMessage}]");
		});
}
