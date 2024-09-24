#if ANDROID || IOS || MACCATALYST || WINDOWS
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

#if ANDROID
using PlatformWindow = Android.App.Activity;
#elif IOS || MACCATALYST
using PlatformWindow = UIKit.UIWindow;
#elif WINDOWS
using PlatformWindow = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Embedding;

/// <summary>
/// A reduced functionality <see cref="WindowHandler"/> for embedded scenarios.
/// </summary>
/// <remarks>
/// The main purpose is to avoid adding mappers that affect the platform window since it is not meant to be
/// modified arbitrarily from embedded MAUI views.
/// </remarks>
internal partial class EmbeddedWindowHandler : ElementHandler<IWindow, PlatformWindow>, IWindowHandler
{
	public static IPropertyMapper<IWindow, IWindowHandler> Mapper =
		new PropertyMapper<IWindow, IWindowHandler>(ElementHandler.ElementMapper)
		{
		};

	public static CommandMapper<IWindow, IWindowHandler> CommandMapper =
		new(ElementCommandMapper)
		{
			[nameof(IWindow.RequestDisplayDensity)] = WindowHandler.MapRequestDisplayDensity,
		};

	public EmbeddedWindowHandler()
		: base(Mapper, CommandMapper)
	{
	}

	public EmbeddedWindowHandler(IPropertyMapper? mapper)
		: base(mapper ?? Mapper, CommandMapper)
	{
	}

	public EmbeddedWindowHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
		: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
	{
	}

	protected override PlatformWindow CreatePlatformElement() =>
		MauiContext!.Services.GetRequiredService<PlatformWindow>() ??
		throw new InvalidOperationException("EmbeddedWindowHandler could not locate a platform window.");
}
#endif
