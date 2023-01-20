using System;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
#if __IOS__ || MACCATALYST
using PlatformView = UIKit.IUIApplicationDelegate;
#elif MONOANDROID
using PlatformView = Android.App.Application;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Application;
#elif TIZEN
using PlatformView = Tizen.Applications.CoreApplication;
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
#pragma warning disable CA1416 // TODO: should we propagate SupportedOSPlatform("ios13.0") here
			[nameof(IApplication.OpenWindow)] = MapOpenWindow,
			[nameof(IApplication.CloseWindow)] = MapCloseWindow,
#pragma warning restore CA1416
		};

		ILogger<ApplicationHandler>? _logger;

		public ApplicationHandler()
			: base(Mapper, CommandMapper)
		{
		}

		public ApplicationHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper)
		{
		}

		public ApplicationHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		ILogger? Logger =>
			_logger ??= MauiContext?.Services.CreateLogger<ApplicationHandler>();

#if !(NETSTANDARD || !PLATFORM)
		protected override PlatformView CreatePlatformElement() =>
			MauiContext?.Services.GetService<PlatformView>() ?? throw new InvalidOperationException($"MauiContext did not have a valid application.");
#endif
	}
}