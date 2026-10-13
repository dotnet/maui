using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace Maui.HybridApp.UI;

/// <summary>Native application-level effective theme/preferences and real OS theme notifications; no web CSS recipes.</summary>
public sealed class ApplicationProvider(HybridWindowHost host, NativeDispatcher dispatcher, NativeNotifications notifications) : IHybridCommandProvider, IDisposable
{
	Application? _application;
	EventHandler<AppThemeChangedEventArgs>? _handler;
	long _epoch;
	bool _active;
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = ApplicationJsonContext.Default;
		registry.Register("application.theme", json.ApplicationEmpty, json.ApplicationTheme, (_, op) =>
			dispatcher.RunAsync(() => Task.FromResult(Read()), op.Cancellation));
		registry.Register("application.setTheme", json.SetThemeArgs, json.ApplicationTheme, (args, op) => dispatcher.RunAsync(() =>
		{
			host.Application.UserAppTheme = args.Theme switch
			{
				"system" => Microsoft.Maui.ApplicationModel.AppTheme.Unspecified,
				"light" => Microsoft.Maui.ApplicationModel.AppTheme.Light,
				"dark" => Microsoft.Maui.ApplicationModel.AppTheme.Dark,
				_ => throw new CommandException("invalid_arguments", "Use system, light or dark application theme."),
			};
			return Task.FromResult(Read());
		}, op.Cancellation));
		registry.RegisterTopic("application.themeChanged", StartAsync, Stop);
	}
	ApplicationTheme Read()
	{
		var app = host.Application;
		static string Effective(Microsoft.Maui.ApplicationModel.AppTheme theme) => theme switch
		{
			Microsoft.Maui.ApplicationModel.AppTheme.Light => "light",
			Microsoft.Maui.ApplicationModel.AppTheme.Dark => "dark",
			_ => "unknown",
		};
		return new(Effective(app.RequestedTheme), app.UserAppTheme == Microsoft.Maui.ApplicationModel.AppTheme.Unspecified ? "system" : Effective(app.UserAppTheme));
	}
	Task StartAsync()
	{
		_active = true; var epoch = Interlocked.Increment(ref _epoch);
		return dispatcher.PostAsync(() =>
		{
			if (!_active || epoch != Volatile.Read(ref _epoch)) return;
			_application = host.Application;
			_handler = (_, _) =>
			{
				if (_active && epoch == Volatile.Read(ref _epoch))
					notifications.Publish("application.themeChanged", Read(), ApplicationJsonContext.Default.ApplicationTheme);
			};
			_application.RequestedThemeChanged += _handler;
		});
	}
	void Stop()
	{
		_active = false; Interlocked.Increment(ref _epoch);
		var app = _application; var handler = _handler;
		_application = null; _handler = null;
		if (app is not null && handler is not null) _ = dispatcher.PostAsync(() => app.RequestedThemeChanged -= handler);
	}
	/// <summary>Detaches this view's native application-theme subscription and ignores stale callbacks.</summary>
	public void Dispose() => Stop();
}
sealed record ApplicationEmpty;
sealed record SetThemeArgs(string Theme);
sealed record ApplicationTheme(string Effective, string Preference);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(ApplicationEmpty))]
[JsonSerializable(typeof(SetThemeArgs))]
[JsonSerializable(typeof(ApplicationTheme))]
partial class ApplicationJsonContext : JsonSerializerContext;
