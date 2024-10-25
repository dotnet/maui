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
	/// <summary>
	/// Represents the view handler for the abstract <see cref="IApplication"/> view and its platform-specific implementation.
	/// </summary>
	/// <seealso href="https://learn.microsoft.com/dotnet/maui/user-interface/handlers/">Conceptual documentation on handlers</seealso>
	public partial class ApplicationHandler
	{
		internal const string TerminateCommandKey = "Terminate";

		public static IPropertyMapper<IApplication, ApplicationHandler> Mapper = new PropertyMapper<IApplication, ApplicationHandler>(ElementMapper)
		{
#if ANDROID || IOS
			[nameof(IApplication.UserAppTheme)] = MapAppTheme,
#endif
		};

		public static CommandMapper<IApplication, ApplicationHandler> CommandMapper = new(ElementCommandMapper)
		{
			[TerminateCommandKey] = MapTerminate,
#pragma warning disable CA1416 // TODO: should we propagate SupportedOSPlatform("ios13.0") here
			[nameof(IApplication.OpenWindow)] = MapOpenWindow,
			[nameof(IApplication.CloseWindow)] = MapCloseWindow,
			[nameof(IApplication.ActivateWindow)] = MapActivateWindow,
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

		/// <summary>
		/// Maps the abstract "Terminate" command to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="application">The associated <see cref="IApplication"/> instance.</param>
		/// <param name="args">The associated command arguments.</param>
		public static partial void MapTerminate(ApplicationHandler handler, IApplication application, object? args);

		/// <summary>
		/// Maps the abstract <see cref="IApplication.OpenWindow"/> command to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="application">The associated <see cref="IApplication"/> instance.</param>
		/// <param name="args">The associated command arguments.</param>
		public static partial void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args);

		/// <summary>
		/// Maps the abstract <see cref="IApplication.CloseWindow"/> command to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="application">The associated <see cref="IApplication"/> instance.</param>
		/// <param name="args">The associated command arguments.</param>
		public static partial void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args);

		/// <summary>
		/// Maps the abstract <see cref="IApplication.ActivateWindow"/> command to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="application">The associated <see cref="IApplication"/> instance.</param>
		/// <param name="args">The associated command arguments.</param>
		public static partial void MapActivateWindow(ApplicationHandler handler, IApplication application, object? args);

#if ANDROID || IOS
		internal static partial void MapAppTheme(ApplicationHandler handler, IApplication application);
#endif
	}
}