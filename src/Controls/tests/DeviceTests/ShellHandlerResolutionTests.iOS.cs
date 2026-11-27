using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[CollectionDefinition(nameof(ShellHandlerResolutionTests), DisableParallelization = true)]
	public sealed class ShellHandlerResolutionTestsCollection
	{
	}

	[Category(TestCategory.Shell)]
	[Collection(nameof(ShellHandlerResolutionTests))]
	public class ShellHandlerResolutionTests
	{
		const string IsiOSShellHandlerEnabledSwitch = "Microsoft.Maui.RuntimeFeature.IsiOSShellHandlerEnabled";

		[Fact]
		public void ShellHandlerSwitchEnabledResolvesShellHandlerHierarchy()
		{
			WithShellHandlerSwitch(true, () =>
			{
				var handlers = CreateHandlersFactory();

				Assert.Equal(typeof(Controls.Handlers.ShellHandler), handlers.GetHandlerType(typeof(Shell)));
				Assert.Equal(typeof(Controls.Handlers.ShellItemHandler), handlers.GetHandlerType(typeof(ShellItem)));
				Assert.Equal(typeof(Controls.Handlers.ShellItemHandler), handlers.GetHandlerType(typeof(TabBar)));
				Assert.Equal(typeof(Controls.Handlers.ShellItemHandler), handlers.GetHandlerType(typeof(FlyoutItem)));
				Assert.Equal(typeof(Controls.Handlers.ShellSectionHandler), handlers.GetHandlerType(typeof(ShellSection)));
				Assert.Equal(typeof(Controls.Handlers.ShellSectionHandler), handlers.GetHandlerType(typeof(Tab)));
				Assert.Equal(typeof(Controls.Handlers.ShellContentHandler), handlers.GetHandlerType(typeof(ShellContent)));

				Assert.IsType<Controls.Handlers.ShellItemHandler>(handlers.GetHandler(typeof(ShellItem)));
				Assert.IsType<Controls.Handlers.ShellSectionHandler>(handlers.GetHandler(typeof(ShellSection)));
				Assert.IsType<Controls.Handlers.ShellContentHandler>(handlers.GetHandler(typeof(ShellContent)));
			});
		}

		[Fact]
		public void ShellHandlerSwitchDisabledResolvesShellRendererOnly()
		{
			WithShellHandlerSwitch(false, () =>
			{
				var handlers = CreateHandlersFactory();

				Assert.Equal(typeof(Controls.Handlers.Compatibility.ShellRenderer), handlers.GetHandlerType(typeof(Shell)));

				// The compatibility renderer creates its own item renderers; no handlers are declared for the hierarchy.
				Assert.Null(handlers.GetHandlerType(typeof(ShellItem)));
				Assert.Null(handlers.GetHandlerType(typeof(TabBar)));
				Assert.Null(handlers.GetHandlerType(typeof(ShellSection)));
				Assert.Null(handlers.GetHandlerType(typeof(ShellContent)));
				Assert.ThrowsAny<Exception>(() => handlers.GetHandler(typeof(ShellItem)));
			});
		}

		static IMauiHandlersFactory CreateHandlersFactory()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<Application>()
				.Build();

			return mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
		}

		static void WithShellHandlerSwitch(bool enabled, Action action)
		{
			var hadValue = AppContext.TryGetSwitch(IsiOSShellHandlerEnabledSwitch, out var originalValue);

			try
			{
				AppContext.SetSwitch(IsiOSShellHandlerEnabledSwitch, enabled);
				action();
			}
			finally
			{
				AppContext.SetSwitch(IsiOSShellHandlerEnabledSwitch, hadValue && originalValue);
			}
		}
	}
}
