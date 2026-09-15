#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;

internal sealed record XamlHotReloadApplicationOptions(
	IReadOnlyDictionary<string, object>? Resources = null,
	AppTheme InitialTheme = AppTheme.Unspecified);

internal sealed class XamlHotReloadHostFixture : IDisposable
{
	readonly Application? _previousApplication;
	readonly Application _application;
	bool _disposed;

	public XamlHotReloadHostFixture(XamlHotReloadApplicationOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);

		_previousApplication = Application.Current;
		_application = new Application();

		if (options.Resources is not null)
		{
			foreach (var resource in options.Resources)
				_application.Resources.Add(resource.Key, resource.Value);
		}

		Application.Current = _application;
		SetAppTheme(options.InitialTheme);
	}

	public Application Application => _application;

	public void SetAppTheme(AppTheme theme)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		_application.UserAppTheme = theme;
	}

	public void Dispatch(Action action)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(action);

		// The unit-test host has no platform run loop; dispatching directly keeps metadata updates
		// deterministic while preserving the synchronous UI-thread contract of the harness API.
		action();
	}

	public void Attach(Page page)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentNullException.ThrowIfNull(page);

#pragma warning disable CS0618
		_application.MainPage = page;
#pragma warning restore CS0618
	}

	public void Detach(Page page)
	{
		ArgumentNullException.ThrowIfNull(page);

#pragma warning disable CS0618
		if (ReferenceEquals(_application.MainPage, page))
			_application.MainPage = null;
#pragma warning restore CS0618
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		Application.Current = _previousApplication;
	}
}
