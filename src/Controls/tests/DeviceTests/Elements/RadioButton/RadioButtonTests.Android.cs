using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonTests
	{
		// RadioButton installs its platform view factory lazily, the first time a RadioButton is attached to a
		// handler. The install is a one-time action per process, so these tests also call InstallPlatformViewFactory
		// directly: other tests may already have attached a RadioButton.
		void SetupTemplatedRadioButtonBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Shape, ShapeViewHandler>();
					handlers.AddHandler<ContentPresenter, ContentViewHandler>();
					handlers.AddHandler<RadioButton, RadioButtonHandler>();
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});
		}

		[Fact(DisplayName = "Controls Installs Its Platform View Factory When The App Has None")]
		public async Task ControlsInstallsItsPlatformViewFactoryWhenTheAppHasNone()
		{
			// Restoring to null would leave the factory unset if this test performed the one-time remap.
			RadioButton.InstallPlatformViewFactory();
			var originalFactory = RadioButtonHandler.PlatformViewFactory;
			try
			{
				RadioButtonHandler.PlatformViewFactory = null;

				RadioButton.InstallPlatformViewFactory();

				Assert.NotNull(RadioButtonHandler.PlatformViewFactory);

				// Only the Controls factory creates a ContentViewGroup for a templated RadioButton.
				SetupTemplatedRadioButtonBuilder();
				var radioButton = new RadioButton { ControlTemplate = RadioButton.DefaultTemplate, Content = "Option" };
				var handler = await CreateHandlerAsync<RadioButtonHandler>(radioButton);

				await InvokeOnMainThreadAsync(() => Assert.IsType<ContentViewGroup>(handler.PlatformView));
			}
			finally
			{
				RadioButtonHandler.PlatformViewFactory = originalFactory;
			}
		}

		[Fact(DisplayName = "Controls Keeps A Platform View Factory The App Installed Before Attaching")]
		public async Task ControlsKeepsAPlatformViewFactoryTheAppInstalledBeforeAttaching()
		{
			// Restoring to null would leave the factory unset if this test performed the one-time remap.
			RadioButton.InstallPlatformViewFactory();
			var originalFactory = RadioButtonHandler.PlatformViewFactory;
			try
			{
				RadioButtonHandler.PlatformViewFactory = null;
				RadioButton.InstallPlatformViewFactory();
				var controlsFactory = RadioButtonHandler.PlatformViewFactory;

				var appFactoryCalls = 0;
				object appPlatformView = null;
				RadioButtonHandler.PlatformViewFactory = handler =>
				{
					appFactoryCalls++;
					var platformView = controlsFactory(handler);
					appPlatformView = platformView;
					return platformView;
				};
				var appFactory = RadioButtonHandler.PlatformViewFactory;

				RadioButton.InstallPlatformViewFactory();

				Assert.Same(appFactory, RadioButtonHandler.PlatformViewFactory);

				SetupTemplatedRadioButtonBuilder();
				var radioButton = new RadioButton { ControlTemplate = RadioButton.DefaultTemplate, Content = "Option" };
				var handler = await CreateHandlerAsync<RadioButtonHandler>(radioButton);

				Assert.Same(appFactory, RadioButtonHandler.PlatformViewFactory);
				Assert.Equal(1, appFactoryCalls);
				Assert.NotNull(appPlatformView);
				Assert.Same(appPlatformView, handler.PlatformView);
			}
			finally
			{
				RadioButtonHandler.PlatformViewFactory = originalFactory;
			}
		}
	}
}
