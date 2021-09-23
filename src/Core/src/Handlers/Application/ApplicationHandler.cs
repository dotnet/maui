using System;
using Microsoft.Extensions.Logging;
#if __IOS__ || MACCATALYST
using NativeView = UIKit.UIApplicationDelegate;
#elif MONOANDROID
using NativeView = Android.App.Application;
#elif WINDOWS
using NativeView = Microsoft.UI.Xaml.Application;
#endif

namespace Microsoft.Maui.Handlers
{
	public partial class ApplicationHandler
	{
		public static IPropertyMapper<IApplication, ApplicationHandler> Mapper = new PropertyMapper<IApplication, ApplicationHandler>(ElementMapper)
		{
		};

		public static CommandMapper<IApplication, ApplicationHandler> CommandMapper = new(ElementCommandMapper)
		{
			[nameof(IApplication.RequestTerminate)] = MapRequestTerminate
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

		ILogger? Logger => _logger ??= MauiContext?.Services.CreateLogger<ApplicationHandler>();

#if !NETSTANDARD
		protected override NativeView CreateNativeElement()
		{
			if (MauiContext is not IMauiApplicationContext applicationContext)
				throw new InvalidOperationException($"{nameof(MauiContext)} was not a {nameof(IMauiApplicationContext)}.");

			var native = applicationContext.
#if __ANDROID__
				Context;
#elif __IOS__
				ApplicationDelegate;
#elif WINDOWS
				Application;
#endif

			return native as NativeView ?? throw new InvalidOperationException($"MauiContext did not have a valid application.");
		}
#endif
	}
}