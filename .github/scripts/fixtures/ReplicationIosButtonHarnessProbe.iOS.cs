#if IOS && !MACCATALYST
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public class ReplicationIosButtonHarnessProbe : ControlsHandlerTestBase
	{
		[Fact]
		[Category("ReplicationIosButtonHarnessProbe")]
		public async Task RegisteredControlsAttachToWindow()
		{
			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<global::Microsoft.Maui.Controls.Button, ButtonHandler>();
				handlers.AddHandler<global::Microsoft.Maui.Controls.Label, LabelHandler>();
			}));

			var button = new Button
			{
				Text = "CI",
				WidthRequest = 64,
				HeightRequest = 64,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			await CreateHandlerAndAddToWindow<ButtonHandler>(
				new Window(new ContentPage { Content = button }), async handler =>
				{
					await AssertEventually(() => button.Handler != null && button.IsLoaded);
					Assert.Same(button.Handler, handler);
					Assert.InRange(button.Width, 63d, 65d);
					Assert.InRange(button.Height, 63d, 65d);
					Assert.NotNull(handler.PlatformView.Window);
					Assert.Equal("CI", handler.PlatformView.CurrentTitle);
				});

			var label = new Label { Text = "Native Label text" };
			await CreateHandlerAndAddToWindow<LabelHandler>(
				new Window(new ContentPage { Content = label }), async handler =>
				{
					await AssertEventually(() => label.Handler != null && label.IsLoaded);
					Assert.Same(label.Handler, handler);
					Assert.NotNull(handler.PlatformView.Window);
					Assert.Equal("Native Label text", handler.PlatformView.Text);
					label.CharacterSpacing = 5d;
					Assert.Equal(5d, handler.PlatformView.AttributedText.GetCharacterSpacing());
				});
		}
	}
}
#endif
