using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.IUIApplicationDelegate;
#elif MONOANDROID
using PlatformView = Android.App.Application;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Application;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler
	{
		internal const string TerminateCommandKey = "Terminate";

		public static IPropertyMapper<IApplication, ApplicationHandler> Mapper = new PropertyMapper<IApplication, ApplicationHandler>(ElementMapper)
		{
		};

		public static CommandMapper<IApplication, ApplicationHandler> CommandMapper = new(ElementCommandMapper)
		{
			[TerminateCommandKey] = MapTerminate,
			[nameof(IApplication.OpenWindow)] = MapOpenWindow,
			[nameof(IApplication.CloseWindow)] = MapCloseWindow,
		};

		ILogger<ApplicationHandler>? _logger;

		public ApplicationHandler()
			: base(Mapper, CommandMapper)
		{
		}

		public ApplicationHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ILogger? Logger =>
			_logger ??= MauiContext?.Services.CreateLogger<ApplicationHandler>();

#if !NETSTANDARD
		protected override PlatformView CreatePlatformElement() =>
			MauiContext?.Services.GetService<PlatformView>() ?? throw new InvalidOperationException($"MauiContext did not have a valid application.");
#endif
	}
}