namespace Microsoft.Maui
{
	/// <summary>
	/// Defines a strongly typed, platform-neutral element handler contract.
	/// </summary>
	/// <typeparam name="TVirtualView">The cross-platform (virtual view) type this handler manages.</typeparam>
	/// <typeparam name="TPlatformView">The native type this handler creates.</typeparam>
	/// <remarks>
	/// <para>
	/// The per-control handler interfaces such as <see cref="Handlers.IWindowHandler"/> declare
	/// <c>PlatformView</c> using a <c>using PlatformView = ...</c> alias that resolves to a different
	/// native type for every target framework. That makes them impossible to implement from an
	/// external platform backend whose native types are not the ones .NET MAUI ships in the box:
	/// the compiler reports <c>CS0738</c> (or <c>CS9333</c> on the platform-neutral target framework)
	/// because the implementing member's return type does not match the alias exactly.
	/// </para>
	/// <para>
	/// This interface has no such alias. Both type parameters are covariant, so a handler can be
	/// matched platform-neutrally by testing for <c>IElementHandler&lt;IWindow, object&gt;</c>
	/// regardless of which native type it actually creates.
	/// </para>
	/// <para>
	/// <see cref="Handlers.ElementHandler{TVirtualView, TPlatformView}"/> implements this interface,
	/// so any handler derived from it — in the box or in an external backend — satisfies the contract
	/// without writing any additional members.
	/// </para>
	/// <para>
	/// <b>Lifecycle.</b> <see cref="VirtualView"/> and <see cref="PlatformView"/> are non-nullable here,
	/// matching the non-nullable properties on
	/// <see cref="Handlers.ElementHandler{TVirtualView, TPlatformView}"/> that satisfy them — and those
	/// properties <b>throw <see cref="System.InvalidOperationException"/></b> rather than returning
	/// <see langword="null"/> once the handler is disconnected.
	/// <see cref="IElementHandler.DisconnectHandler"/> clears the platform view before invoking the
	/// disconnect callback, so a handler is only guaranteed to have one between
	/// <see cref="IElementHandler.SetVirtualView"/> and <see cref="IElementHandler.DisconnectHandler"/>.
	/// The inherited <see cref="IElementHandler.VirtualView"/> and
	/// <see cref="IElementHandler.PlatformView"/> return <see langword="null"/> in exactly that state and
	/// never throw, so code that can run outside a connected window — teardown paths, weak-reference
	/// caches, diagnostics — should read those instead. Property and command mappers always run while
	/// the handler is connected and can use the typed members freely.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code lang="csharp"><![CDATA[
	/// // In an external platform backend, with a native type MAUI knows nothing about:
	/// public class MyWindowHandler : ElementHandler<IWindow, MyNativeWindow>
	/// {
	///     public MyWindowHandler() : base(WindowHandler.Mapper) { }
	///     protected override MyNativeWindow CreatePlatformElement() => new MyNativeWindow();
	/// }
	///
	/// // Consumers match it platform-neutrally. The typed members below throw once the handler is
	/// // disconnected, so guard on the nullable IElementHandler.PlatformView first.
	/// if (window.Handler is { PlatformView: not null } handler &&
	///     handler is IElementHandler<IWindow, object> windowHandler)
	/// {
	///     IWindow virtualView = windowHandler.VirtualView;
	///     object nativeWindow = windowHandler.PlatformView;
	/// }
	/// ]]></code>
	/// </example>
	public interface IElementHandler<out TVirtualView, out TPlatformView> : IElementHandler
		where TVirtualView : IElement
		where TPlatformView : class
	{
		/// <summary>
		/// Gets the cross-platform virtual view associated with the handler.
		/// </summary>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> when the handler is not connected. Read
		/// the nullable <see cref="IElementHandler.VirtualView"/> instead on paths that can run while
		/// disconnected.
		/// </remarks>
		new TVirtualView VirtualView { get; }

		/// <summary>
		/// Gets the native view associated with the handler.
		/// </summary>
		/// <remarks>
		/// Throws <see cref="System.InvalidOperationException"/> when the handler is not connected. Read
		/// the nullable <see cref="IElementHandler.PlatformView"/> instead on paths that can run while
		/// disconnected.
		/// </remarks>
		new TPlatformView PlatformView { get; }
	}
}
