using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Compatibility)]
	public partial class VisualElementRendererTests : ControlsHandlerTestBase
	{
		class LegacyComponent : View
		{

		}

#if WINDOWS
		class LegacyComponentRenderer : ViewRenderer<View, UI.Xaml.FrameworkElement>
#else
		class LegacyComponentRenderer : VisualElementRenderer<LegacyComponent>
#endif
		{
#if ANDROID
			public LegacyComponentRenderer(global::Android.Content.Context context) : base(context)
			{
				
			}
#endif
		}


		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<LegacyComponent, LegacyComponentRenderer>();
				});
			});
		}

		[Fact]
		public async Task CompatibilityRendererWorksWithNoInnerContrlSpecified()
		{
			SetupBuilder();

			var renderer = await InvokeOnMainThreadAsync(() => new LegacyComponent().ToPlatform(MauiContext));

			await InvokeOnMainThreadAsync(() => Assert.Equal(renderer, (renderer as IPlatformViewHandler).PlatformView));

			Assert.NotNull(renderer);
		}
	}
}