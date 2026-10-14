using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Layouts;
using AbsoluteLayout = Microsoft.Maui.Controls.AbsoluteLayout;

namespace Maui.HybridApp.UI;

/// <summary>Public HTTPS top-level navigation policy; not a DNS or subresource firewall.</summary>
public static class RemoteWebAddress
{
	/// <summary>Rejects credentials, local/IP/bridge destinations and unsafe schemes.</summary>
	/// <param name="value">An absolute public HTTPS URL of at most 2048 ASCII characters.</param>
	/// <returns>The validated address.</returns>
	public static Uri Validate(string value)
	{
		if (value.Length is < 1 or > 2048 || value.Any(c => c <= ' ' || c > 127 || c == '\\') ||
			!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
			uri.UserInfo.Length != 0 || uri.Port != 443 || IPAddress.TryParse(uri.Host, out _) ||
			uri.HostNameType != UriHostNameType.Dns || !uri.Host.Contains('.', StringComparison.Ordinal) ||
			uri.Host.EndsWith(".", StringComparison.Ordinal) ||
			uri.Host.Split('.').Any(label => label is "localhost" or "local" or "internal" or "test" or "invalid"))
			throw new CommandException("invalid_url", "Use public HTTPS without credentials, local/IP addresses or custom ports.");
		return uri;
	}
}

/// <summary>A scoped desktop browser guest with explicit geometry, navigation and teardown, never a command target.</summary>
[HybridTopic("remote.changed")]
public sealed class RemoteWebViewProvider(HybridViewContext context, IDeviceInfo device, NativeNotifications notifications)
{
	bool Supported => device.Platform == DevicePlatform.MacCatalyst || device.Platform == DevicePlatform.WinUI;

	[HybridAction("remote.info", MainThread = true)]
	internal RemoteState Info() => State();

	[HybridAction("remote.open", MainThread = true)]
	internal RemoteState Open(string url)
	{
		var uri = RemoteWebAddress.Validate(url);
		RequireDesktop();
		context.ShowRemote(uri, () => notifications.Publish("remote.changed", State(), RemoteWebViewJsonContext.Default.RemoteState));
		return State();
	}
	[HybridAction("remote.bounds", MainThread = true)]
	internal RemoteState Bounds(string id, double x, double y, double width, double height, double clipX, double clipY,
		double clipWidth, double clipHeight, double viewportWidth, double viewportHeight, bool visible)
	{
		RequireDesktop();
		context.Layout.Place(new(id, x, y, width, height, clipX, clipY, clipWidth, clipHeight, viewportWidth, viewportHeight, visible));
		return State();
	}
	[HybridAction("remote.control", MainThread = true)]
	internal RemoteState Control(string id, string action)
	{
		RequireDesktop();
		context.Layout.Control(id, action);
		return State();
	}
	[HybridAction("remote.navigate", MainThread = true)]
	internal RemoteState Navigate(string id, string url)
	{
		RequireDesktop();
		context.Layout.Navigate(id, RemoteWebAddress.Validate(url));
		return State();
	}

	void RequireDesktop()
	{
		if (!Supported)
			throw new NotSupportedException("This native browser-guest proof supports Mac Catalyst and Windows only.");
	}
	RemoteState State() => context.Layout.Read(Supported);
}

/// <summary>Full-size trusted host with a clipped ordinary native WebView guest above its measured DOM slot.</summary>
/// <remarks>Native composition is not a DOM element. Geometry is measured in CSS coordinates and scaled to the actual host layout, including zoom/DPI changes.</remarks>
public sealed class HybridViewLayout : AbsoluteLayout, IDisposable
{
	AbsoluteLayout? _clip;
	WebView? _web;
	Action? _changed;
	string? _id;
	string? _url;
	string _status = "closed";
	bool _disposed;

	/// <summary>Creates the full-size native host without changing the frontend's layout.</summary>
	/// <param name="primary">The trusted caller-owned view.</param>
	public HybridViewLayout(View primary)
	{
		Add(primary);
		SetLayoutBounds((BindableObject)primary, new Microsoft.Maui.Graphics.Rect(0, 0, 1, 1));
		SetLayoutFlags((BindableObject)primary, AbsoluteLayoutFlags.All);
		SizeChanged += Resized;
	}

	void Resized(object? sender, EventArgs args)
	{
		if (_clip is not null)
			_clip.IsVisible = false;
	}

	internal void Show(Uri address, Action changed)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		Close();
		_id = Guid.NewGuid().ToString("D");
		_url = address.AbsoluteUri;
		_status = "loading";
		_changed = changed;
		_clip = new AbsoluteLayout { IsClippedToBounds = true, IsVisible = false };
		_web = new WebView { AutomationId = "remote-webview" };
		_web.Navigating += Navigating;
		_web.Navigated += Navigated;
		_clip.Add(_web);
		Add(_clip);
		_web.Source = address.AbsoluteUri;
	}

	internal RemoteState Read(bool supported) => new(supported, _web is not null, _id, _url,
		_status, _web?.CanGoBack ?? false, _web?.CanGoForward ?? false);

	internal void Place(RemoteBounds args)
	{
		RequireId(args.Id);
		var values = new[] { args.X, args.Y, args.Width, args.Height, args.ClipX, args.ClipY, args.ClipWidth,
			args.ClipHeight, args.ViewportWidth, args.ViewportHeight };
		if (!args.Visible)
		{
			_clip!.IsVisible = false;
			return;
		}
		if (values.Any(value => !double.IsFinite(value) || Math.Abs(value) > 20000) ||
			args.Width < 0 || args.Height < 0 || args.ClipWidth < 0 || args.ClipHeight < 0 ||
			args.ViewportWidth <= 0 || args.ViewportHeight <= 0 ||
			args.ClipX < 0 || args.ClipY < 0 || args.ClipX + args.ClipWidth > args.ViewportWidth + 1 ||
			args.ClipY + args.ClipHeight > args.ViewportHeight + 1 ||
			args.ClipX < args.X || args.ClipY < args.Y ||
			args.ClipX + args.ClipWidth > args.X + args.Width + 1 ||
			args.ClipY + args.ClipHeight > args.Y + args.Height + 1)
			throw new CommandException("invalid_arguments", "Supply finite viewport-clipped DOM geometry within 20000 CSS pixels.");
		if (!args.Visible || args.ClipWidth < 2 || args.ClipHeight < 2 || Width <= 0 || Height <= 0)
		{
			_clip!.IsVisible = false;
			return;
		}
		var sx = Width / args.ViewportWidth;
		var sy = Height / args.ViewportHeight;
		SetLayoutBounds((BindableObject)_clip!, new Microsoft.Maui.Graphics.Rect(args.ClipX * sx, args.ClipY * sy, args.ClipWidth * sx, args.ClipHeight * sy));
		SetLayoutBounds((BindableObject)_web!, new Microsoft.Maui.Graphics.Rect((args.X - args.ClipX) * sx, (args.Y - args.ClipY) * sy, args.Width * sx, args.Height * sy));
		_clip!.IsVisible = true;
	}

	internal void Control(string id, string action)
	{
		RequireId(id);
		switch (action)
		{
			case "back": if (_web!.CanGoBack) _web.GoBack(); break;
			case "forward": if (_web!.CanGoForward) _web.GoForward(); break;
			case "reload": _web!.Reload(); break;
			case "focus": if (!_web!.Focus()) throw new CommandException("focus_unavailable", "Native browser focus was not accepted."); break;
			case "hide": _clip!.IsVisible = false; break;
			case "close": Close(); break;
			default: throw new CommandException("invalid_arguments", "Unknown browser control.");
		}
	}


	void RequireId(string id)
	{
		if (_disposed || _id is null || id != _id)
			throw new CommandException("invalid_view", "This browser guest does not belong to the calling live view.");
	}
	internal void Navigate(string id, Uri address)
	{
		RequireId(id);
		_url = address.AbsoluteUri; _status = "loading";
		_web!.Source = address.AbsoluteUri;
	}
	void Navigating(object? sender, WebNavigatingEventArgs e)
	{
		try { _url = RemoteWebAddress.Validate(e.Url).AbsoluteUri; _status = "loading"; }
		catch (CommandException) { e.Cancel = true; _status = "blocked: only public HTTPS navigation is allowed"; }
		_changed?.Invoke();
	}
	void Navigated(object? sender, WebNavigatedEventArgs e)
	{
		_status = e.Result == WebNavigationResult.Success ? "loaded" : $"failed: {e.Result}";
		_changed?.Invoke();
	}

	internal void Close()
	{
		if (_web is not null)
		{
			_web.Navigating -= Navigating;
			_web.Navigated -= Navigated;
			_web.Handler?.DisconnectHandler();
		}
		if (_clip is not null)
			Remove(_clip);
		_clip = null;
		_web = null;
		_id = null;
		_url = null;
		_status = "closed";
		_changed?.Invoke();
		_changed = null;
	}

	/// <summary>Disconnects the guest and detaches resize notifications.</summary>
	public void Dispose()
	{
		Close();
		SizeChanged -= Resized;
		_disposed = true;
	}
}

sealed record RemoteBounds(string Id, double X, double Y, double Width, double Height,
	double ClipX, double ClipY, double ClipWidth, double ClipHeight, double ViewportWidth, double ViewportHeight, bool Visible);
sealed record RemoteState(bool Supported, bool Open, string? Id, string? Url, string Status, bool CanGoBack, bool CanGoForward);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(RemoteState))]
partial class RemoteWebViewJsonContext : JsonSerializerContext;
