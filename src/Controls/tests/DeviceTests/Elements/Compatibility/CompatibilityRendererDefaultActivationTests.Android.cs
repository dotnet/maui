using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Compatibility)]
	public class CompatibilityRendererDefaultActivationTests : ControlsHandlerTestBase
	{
		public static TheoryData<Func<View>, string> DefaultCompatibilityRenderers => new()
		{
			{ () => new ListView(), "Microsoft.Maui.Controls.Handlers.Compatibility.ListViewRenderer" },
			{ () => new TableView(), "Microsoft.Maui.Controls.Handlers.Compatibility.TableViewRenderer" },
			{ () => new Frame(), "Microsoft.Maui.Controls.Handlers.Compatibility.FrameRenderer" },
		};

		// These renderers only have constructors that take an Android Context, so their [ElementHandler]
		// defaults are activated with the current context's Context rather than a parameterless constructor.
		[Theory]
		[MemberData(nameof(DefaultCompatibilityRenderers))]
		public async Task DefaultRendererIsActivatedWithCurrentContext(Func<View> createView, string rendererTypeName)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var mauiApp = MauiApp.CreateBuilder()
					.UseMauiApp<Application>()
					.Build();
				var mauiContext = new ContextStub(mauiApp.Services);

				var handler = createView().ToHandler(mauiContext);

				Assert.Equal(rendererTypeName, handler.GetType().FullName);
				var platformView = Assert.IsAssignableFrom<AView>(Assert.IsAssignableFrom<IPlatformViewHandler>(handler).PlatformView);
				Assert.NotNull(mauiContext.Context);
				Assert.Same(mauiContext.Context, platformView.Context);
			});
		}
	}
}
