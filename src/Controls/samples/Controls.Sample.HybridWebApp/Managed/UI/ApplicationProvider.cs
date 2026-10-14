using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;
using AppTheme = Microsoft.Maui.ApplicationModel.AppTheme;

namespace Maui.HybridApp.UI;

/// <summary>Native application-level effective theme/preferences and real OS theme notifications; no web CSS recipes.</summary>
public sealed class ApplicationProvider(HybridWindowHost host, NativeDispatcher dispatcher, NativeNotifications notifications) : IDisposable
{
	Application? _application;
	EventHandler<AppThemeChangedEventArgs>? _handler;
	long _epoch;
	bool _active;
	[HybridAction("application.theme", MainThread = true)]
	internal ApplicationTheme Theme() => Read();

	[HybridAction("application.setTheme", MainThread = true)]
	internal ApplicationTheme SetTheme(AppTheme theme)
	{
		host.Application.UserAppTheme = theme;
		return Read();
	}
	ApplicationTheme Read()
	{
		var app = host.Application;
		return new(app.RequestedTheme, app.UserAppTheme);
	}
	[HybridEvent("application.themeChanged", HybridEventLifecycle.Start)]
	internal Task StartAsync()
	{
		_active = true; var epoch = Interlocked.Increment(ref _epoch);
		return dispatcher.PostAsync(() =>
		{
			if (!_active || epoch != Volatile.Read(ref _epoch)) return;
			_application = host.Application;
			_handler = (_, _) =>
			{
				if (_active && epoch == Volatile.Read(ref _epoch))
					notifications.PublishCurrent("application.themeChanged", Read(), ApplicationJsonContext.Wire.ApplicationTheme,
						() => Volatile.Read(ref _active) && epoch == Volatile.Read(ref _epoch));
			};
			_application.RequestedThemeChanged += _handler;
		});
	}
	[HybridEvent("application.themeChanged", HybridEventLifecycle.Stop)]
	internal void Stop()
	{
		_active = false; Interlocked.Increment(ref _epoch);
		var app = _application; var handler = _handler;
		_application = null; _handler = null;
		if (app is not null && handler is not null) _ = dispatcher.PostAsync(() => app.RequestedThemeChanged -= handler);
	}
	/// <summary>Detaches this view's native application-theme subscription and ignores stale callbacks.</summary>
	public void Dispose() => Stop();
}
sealed record ApplicationTheme(AppTheme Effective, AppTheme Preference);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ApplicationTheme))]
partial class ApplicationJsonContext : JsonSerializerContext
{
	internal static ApplicationJsonContext Wire { get; } = new(WireOptions());
	static System.Text.Json.JsonSerializerOptions WireOptions()
	{
		var options = new System.Text.Json.JsonSerializerOptions
		{
			PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
			UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
			RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true,
		};
		options.Converters.Add(new JsonStringEnumConverter<AppTheme>(System.Text.Json.JsonNamingPolicy.CamelCase, allowIntegerValues: false));
		return options;
	}
}
