using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.HybridWebView)]
	public class HybridWebViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
				});
			});
		}

		[Fact]
		public async Task LoadsHtmlAndSendReceiveRawMessage()
		{
#if ANDROID
			// NOTE: skip this test on older Android devices because it is not currently supported on these versions
			if (!System.OperatingSystem.IsAndroidVersionAtLeast(24))
			{
				return;
			}
#endif

			SetupBuilder();

			await InvokeOnMainThreadAsync(async () =>
			{
				var hybridWebView = new HybridWebView()
				{
					WidthRequest = 100,
					HeightRequest = 100,

					HybridRoot = "HybridTestRoot",
				};

				var lastRawMessage = "";

				hybridWebView.RawMessageReceived += (s, e) =>
				{
					lastRawMessage = e.Message;
				};

				var handler = CreateHandler(hybridWebView);

				var platformView = handler.PlatformView;

				// Setup the view to be displayed/parented and run our tests on it
				await AttachAndRun(hybridWebView, async (handler) =>
				{
					await Task.Delay(5000);

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
			});
		}
	}
}
