using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests;

public class Issue29282Tests : global::Microsoft.Maui.DeviceTests.ControlsHandlerTestBase
{
	[Fact]
	[Category("Issue29282")]
	public async global::System.Threading.Tasks.Task HtmlEncodedLessThanFollowedByCharacterRendersCompleteText()
	{
		EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers => handlers.AddHandler<global::Microsoft.Maui.Controls.Label, global::Microsoft.Maui.Handlers.LabelHandler>()));
		var affectedLabel = new global::Microsoft.Maui.Controls.Label { Text = "Does not work => <span>&lt</span>a", TextType = global::Microsoft.Maui.TextType.Html };
		var applyReportedTrigger = true;
		if (applyReportedTrigger)
		{
			affectedLabel.Text = "Does not work => &lt;a";
		}
		await CreateHandlerAndAddToWindow<global::Microsoft.Maui.Handlers.LabelHandler>(new global::Microsoft.Maui.Controls.Window(new global::Microsoft.Maui.Controls.ContentPage { Content = affectedLabel }), async handler => { await AssertEventually(() => affectedLabel.Handler != null && affectedLabel.IsLoaded); Assert.Equal("Does not work => <a", handler.PlatformView.Text); });
	}
}

