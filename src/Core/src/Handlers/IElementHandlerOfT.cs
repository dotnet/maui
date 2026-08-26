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
	/// // Consumers match it platform-neutrally:
	/// if (window.Handler is IElementHandler<IWindow, object> windowHandler)
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
		new TVirtualView VirtualView { get; }

		/// <summary>
		/// Gets the native view associated with the handler.
		/// </summary>
		new TPlatformView PlatformView { get; }
	}
}
