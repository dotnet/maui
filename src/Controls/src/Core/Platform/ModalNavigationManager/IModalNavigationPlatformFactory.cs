#nullable enable

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Application-wide factory for creating <see cref="IModalNavigationPlatform"/> instances.
	/// Register an implementation in the application's
	/// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to replace the built-in
	/// modal presentation for each window the factory chooses to handle.
	/// </summary>
	/// <remarks>
	/// <para>
	/// When an <see cref="IModalNavigationPlatformFactory"/> is present in
	/// <see cref="IMauiContext.Services"/> for a window, the framework calls
	/// <see cref="CreateModalNavigationPlatform(IModalNavigationHost)"/> once for that window and routes
	/// every modal push and pop to the returned instance instead of the built-in platform code. This
	/// lets alternative platform backends render modal navigation without reflection, partial classes or
	/// forking, while the framework keeps ownership of the cross-platform modal stack, the page
	/// lifecycle events and the push/pop reconciliation loop.
	/// </para>
	/// <para>
	/// The factory is resolved from the window's service scope, so it may be registered with any
	/// lifetime. The framework calls it once per window and disposes the returned instance, so return a
	/// new <see cref="IModalNavigationPlatform"/> for each call. It is called again for the same window
	/// only if the window gets a new handler, which brings a new service scope (for example an Android
	/// activity recreation).
	/// </para>
	/// <para>
	/// If this method throws, the framework logs the exception and permanently falls back to the
	/// built-in platform for that window rather than rethrowing from whatever call site happened to
	/// trigger resolution. Creation is not retried, so a failed registration fails deterministically and
	/// visibly in the log instead of throwing repeatedly from arbitrary navigation code. Prefer
	/// returning <see langword="null"/> over throwing when a backend deliberately does not want to
	/// handle a window.
	/// </para>
	/// <example>
	/// Registering a backend that presents modals with its own native window stack:
	/// <code lang="csharp">
	/// public sealed class MyModalNavigationPlatformFactory : IModalNavigationPlatformFactory
	/// {
	///     public IModalNavigationPlatform? CreateModalNavigationPlatform(IModalNavigationHost host)
	///         =&gt; new MyModalNavigationPlatform(host);
	/// }
	///
	/// public sealed class MyModalNavigationPlatform : IModalNavigationPlatform
	/// {
	///     readonly IModalNavigationHost _host;
	///     readonly MyNativeModalStack _stack;
	///
	///     public MyModalNavigationPlatform(IModalNavigationHost host)
	///     {
	///         _host = host;
	///         // Captured while the instance is live: Dispose must not depend on the MauiContext.
	///         _stack = MyNativeModalStack.For(host.MauiContext);
	///     }
	///
	///     public bool IsReady =&gt; _host.IsWindowReady &amp;&amp; _stack.IsRealized;
	///
	///     public async Task PushModalAsync(Page modal, bool animated)
	///     {
	///         var nativeView = modal.ToPlatform(_host.MauiContext);
	///         await _stack.PushAsync(nativeView, animated &amp;&amp; !_host.IsBatchPushing);
	///     }
	///
	///     public async Task PopModalAsync(Page modal, bool animated)
	///     {
	///         // Idempotent: the modal may already be gone when the user dismissed it natively.
	///         if (_stack.Contains(modal))
	///             await _stack.PopAsync(animated &amp;&amp; !_host.IsBatchPopping);
	///
	///         modal.DisconnectHandlers();
	///     }
	///
	///     public void PageAttached() =&gt;
	///         _stack.BackButtonPressed = () =&gt; _host.CurrentPage?.SendBackButtonPressed() ?? false;
	///
	///     // A native dismissal the framework can't see has to be routed back through Navigation so the
	///     // cross-platform stack and the page lifecycle events stay in sync.
	///     void OnDismissedNatively() =&gt; _host.Window.Navigation.PopModalAsync(animated: false);
	///
	///     public void Dispose()
	///     {
	///         // Still-presented modals are NOT popped by the framework during teardown.
	///         _stack.DismissAll();
	///     }
	/// }
	///
	/// // In MauiProgram:
	/// builder.Services.AddSingleton&lt;IModalNavigationPlatformFactory, MyModalNavigationPlatformFactory&gt;();
	/// </code>
	/// </example>
	/// </remarks>
	public interface IModalNavigationPlatformFactory
	{
		/// <summary>
		/// Creates the <see cref="IModalNavigationPlatform"/> for a window.
		/// </summary>
		/// <param name="host">
		/// The per-window host that exposes the framework's modal navigation state. Keep a reference to
		/// it; it stays valid for the lifetime of the returned instance.
		/// </param>
		/// <returns>
		/// A new <see cref="IModalNavigationPlatform"/> owned and disposed by the framework, or
		/// <see langword="null"/> to let the window keep the built-in platform implementation. Returning
		/// <see langword="null"/> is useful when a backend only wants to override modal navigation for
		/// certain windows.
		/// </returns>
		IModalNavigationPlatform? CreateModalNavigationPlatform(IModalNavigationHost host);
	}
}
