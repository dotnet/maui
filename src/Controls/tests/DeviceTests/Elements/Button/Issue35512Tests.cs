#if MACCATALYST
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class Issue35512 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ResettingBackgroundColorRestoresInitialNativeAppearance()
		{
			var buttonStyle = new Style(typeof(Button))
			{
				Setters =
				{
					new Setter
					{
						Property = Button.BackgroundColorProperty,
						Value = Colors.Purple
					}
				}
			};
			var button = new Button
			{
				Text = "Background target",
				HeightRequest = 100,
				WidthRequest = 200,
				Style = buttonStyle
			};

			await AttachAndRun<ButtonHandler>(button, handler =>
			{
				var nativeButton = handler.PlatformView;
				var initialBackground =
					(nativeButton.Configuration?.BaseBackgroundColor ?? nativeButton.BackgroundColor)?.ToColor();

				button.BackgroundColor = Colors.Red;
				button.BackgroundColor = null;
				var restoredBackground =
					(nativeButton.Configuration?.BaseBackgroundColor ?? nativeButton.BackgroundColor)?.ToColor();

				Assert.True(
					object.Equals(initialBackground, restoredBackground),
					"Button native background should match its initial value after BackgroundColor is reset to null.");
			});
		}
	}
}
#endif
