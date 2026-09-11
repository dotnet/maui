#if IOS && !MACCATALYST
using System.Linq;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public class Issue38291Tests : global::Microsoft.Maui.DeviceTests.ControlsHandlerTestBase
	{
		[Fact]
		[Category("Issue38291")]
		public async global::System.Threading.Tasks.Task NumberOfTapsRequiredUpdatesAfterAttachment()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<global::Microsoft.Maui.Controls.Label, global::Microsoft.Maui.Handlers.LabelHandler>()));
			var affectedTap = new global::Microsoft.Maui.Controls.TapGestureRecognizer { NumberOfTapsRequired = 1 };
			var affectedLabel = new global::Microsoft.Maui.Controls.Label { Text = "Tap pad", GestureRecognizers = { affectedTap, new global::Microsoft.Maui.Controls.PointerGestureRecognizer() } };
			var applyReportedTrigger = true;
			if (applyReportedTrigger)
			{
				affectedTap.NumberOfTapsRequired = 2;
			}
			await CreateHandlerAndAddToWindow<global::Microsoft.Maui.Handlers.LabelHandler>(new global::Microsoft.Maui.Controls.Window(new global::Microsoft.Maui.Controls.ContentPage { Content = affectedLabel }), async handler =>
			{
				await AssertEventually(() => affectedLabel.Handler != null && affectedLabel.IsLoaded);
				affectedTap.NumberOfTapsRequired = 1;
				Assert.Equal((nuint)1, handler.PlatformView.GestureRecognizers.OfType<global::UIKit.UITapGestureRecognizer>().Single().NumberOfTapsRequired);
			});
		}
	}
}
#endif

