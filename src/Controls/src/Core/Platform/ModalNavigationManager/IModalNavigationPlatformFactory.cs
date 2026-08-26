#nullable enable

namespace Microsoft.Maui.Controls.Platform
{
	/// <summary>
	/// Application-wide factory for creating <see cref="IModalNavigationPlatform"/> instances.
	/// Register an implementation in the application's
	/// <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/> to replace the built-in
	/// modal presentation for every window.
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
	/// The factory itself is resolved from the window's service scope, so it may be registered with any
	/// lifetime. The framework always calls it once per window and disposes the returned instance, so
	/// return a new <see cref="IModalNavigationPlatform"/> for each call.
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
	///
	///     public MyModalNavigationPlatform(IModalNavigationHost host) =&gt; _host = host;
	///
	///     public bool IsReady =&gt; true;
	///
	///     public async Task PushModalAsync(Page modal, bool animated)
	///     {
	///         var nativeView = modal.ToPlatform(_host.MauiContext);
	///         await MyNativeModalStack.For(_host.Window).PushAsync(nativeView, animated);
	///     }
	///
	///     public async Task PopModalAsync(Page modal, bool animated)
	///     {
	///         await MyNativeModalStack.For(_host.Window).PopAsync(animated &amp;&amp; !_host.IsBatchPopping);
	///         (modal.Handler as IPlatformViewHandler)?.Dispose();
	///     }
	///
	///     public void PageAttached()
	///     {
	///         MyNativeWindow.For(_host.Window).BackButtonPressed =
	///             () =&gt; _host.CurrentPage?.SendBackButtonPressed() ?? false;
	///     }
	///
	///     public void Dispose() =&gt; MyNativeModalStack.Release(_host.Window);
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
