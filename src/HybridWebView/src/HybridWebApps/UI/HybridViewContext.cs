using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.HybridWebApps.UI;

/// <summary>A single owning page/window binding, independent of provider construction.</summary>
/// <remarks>Bind once before attaching the WebView handler. Dispose with its owning scope; never resolve from the root.</remarks>
public sealed class HybridViewContext : IDisposable
{
	readonly SemaphoreSlim _interaction = new(1, 1);
	Page? _page;
	Window? _window;
	HybridWebView? _primary;
	bool _disposed;
	HybridViewLayout? _layout;
	long _remoteGeneration;
	/// <summary>Raised once after the host supplies its owning page/window, before native handlers attach.</summary>
	public event Action? Bound;
	/// <summary>Raised when the owning frontend document retires; native feature resources must unbind stale JS installations.</summary>
	public event Action? DocumentEnded;

	/// <summary>Gets the unique identity of this view's native window, not its document or process.</summary>
	public string Id { get; private set; } = Guid.NewGuid().ToString("D");
	internal string Lease { get; } = Guid.NewGuid().ToString("D");
	internal Microsoft.Maui.Dispatching.IDispatcher TeardownDispatcher => !_disposed && _window is not null ? _window.Dispatcher :
		throw new CommandException("context_unavailable", "The owning dispatcher is unavailable.");

	/// <summary>Gets the host-validated initial bundled location for frontend bootstrap.</summary>
	public string InitialLocation { get; private set; } = "index.html";

	/// <summary>Gets the owning live page, never Application.Current.MainPage.</summary>
	public Page Page
	{
		get
		{
			if (!_disposed && _page is not null)
			{
				if (_primary is null && ReferenceEquals(_page.Window, _window))
					return _page;
				for (Element? current = _primary; current is not null; current = current.Parent)
					if (current is Page page && ReferenceEquals(page.Window, _window))
						return page;
			}
			throw new CommandException("context_unavailable", "The owning native page is detached or unavailable.");
		}
	}

	/// <summary>Gets the owning live window.</summary>
	public Window Window => Page.Window ??
		throw new CommandException("context_unavailable", "The owning native window is not available.");

	/// <summary>Gets the currently attached primary view, never another window's view.</summary>
	public HybridWebView WebView { get { _ = Page; return _primary ?? throw new CommandException("context_unavailable", "No primary hybrid view is bound."); } }

	/// <summary>Creates this view's native layout slot for isolated remote content, without HTML coordinates or overlays.</summary>
	/// <param name="primary">The trusted host view, normally a HybridWebView.</param>
	/// <returns>A full-size native composition host with a separate clipped remote surface.</returns>
	public HybridViewLayout CreateLayout(View primary)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (_layout is not null)
			throw new InvalidOperationException("This view's native content is already bound.");
		_primary = primary as HybridWebView;
		return _layout = new HybridViewLayout(primary);
	}

	internal HybridViewLayout Layout => _layout ?? throw new CommandException("configuration_required", "Bind a native host layout first.");


	internal void ShowRemote(Uri address, Action stateChanged)
	{
		_ = Page;
		Interlocked.Increment(ref _remoteGeneration);
		Layout.Show(address, stateChanged);
	}

	/// <summary>Closes remote content on document disconnect; delayed teardown cannot close a newer presentation.</summary>
	public void DisconnectRemote()
	{
		var generation = Interlocked.Increment(ref _remoteGeneration);
		Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
		{
			if (generation == Volatile.Read(ref _remoteGeneration))
				_layout?.Close();
		});
	}
	internal void RetireDocument() => DocumentEnded?.Invoke();

	/// <summary>Binds this scope once to the host-created page/window and trusted bundled location.</summary>
	/// <param name="page">The page resolving this scope's hub.</param>
	/// <param name="window">Its owning window.</param>
	/// <param name="location">Frontend-owned bundle entry URL already validated by the host.</param>
	/// <param name="id">Optional immutable developer window ID; omitted to use an automatically generated identity.</param>
	public void Bind(Page page, Window window, string location, string? id = null)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		if (_page is not null)
			throw new InvalidOperationException("A view scope can only be bound once.");
		if (!ReferenceEquals(page.Window, window) ||
			(!ReferenceEquals(window.Page, page) && !(window.Page is NavigationPage navigation && ReferenceEquals(navigation.CurrentPage, page))))
			throw new ArgumentException("The page must belong to the supplied window.");
		var identity = id is not null ? WindowIdentity.Validate(id) : Id;
		var bundledLocation = BundledLocation.Validate(location);
		_page = page;
		_window = window;
		Id = identity;
		InitialLocation = bundledLocation;
		Bound?.Invoke();
	}

	internal async Task<T> InteractAsync<T>(Func<Task<T>> action, CancellationToken cancellation)
	{
		await _interaction.WaitAsync(cancellation);
		try
		{
			_ = Page;
			cancellation.ThrowIfCancellationRequested();
			// MAUI's stock dialogs/picker cannot be programmatically dismissed. Keep the
			// slot until actual dismissal even when the hub cancels its frontend wait.
			return await action();
		}
		finally
		{
			_interaction.Release();
		}
	}

	/// <summary>Releases page/window references; pending OS UI results cannot be applied by the cancelled hub.</summary>
	public void Dispose()
	{
		_disposed = true;
		_page = null;
		_window = null;
		_primary = null;
		_layout?.Dispose();
		_layout = null;
		Bound = null;
		DocumentEnded = null;
	}
}
