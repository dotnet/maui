namespace Microsoft.Maui
{
	/// <summary>
	/// Defines a strongly typed, platform-neutral view handler contract.
	/// </summary>
	/// <typeparam name="TVirtualView">The cross-platform (virtual view) type this handler manages.</typeparam>
	/// <typeparam name="TPlatformView">The native type this handler creates.</typeparam>
	/// <remarks>
	/// <para>
	/// This is the <see cref="IViewHandler"/> counterpart of
	/// <see cref="IElementHandler{TVirtualView, TPlatformView}"/>. Unlike the per-control handler
	/// interfaces such as <see cref="Handlers.ILabelHandler"/>, <see cref="Handlers.IContentViewHandler"/>
	/// or <see cref="ILayoutHandler"/>, it does not bind <c>PlatformView</c> to a per-target-framework
	/// type alias, so an external platform backend can implement it with its own native types.
	/// </para>
	/// <para>
	/// Both type parameters are covariant. Use <c>object</c> for <typeparamref name="TPlatformView"/>
	/// to match any handler for a given virtual view type, regardless of backend.
	/// </para>
	/// <para>
	/// <see cref="Handlers.ViewHandler{TVirtualView, TPlatformView}"/> implements this interface, so
	/// every handler derived from it already satisfies the contract with no extra members.
	/// </para>
	/// <para>
	/// <b>Lifecycle.</b> As with
	/// <see cref="IElementHandler{TVirtualView, TPlatformView}"/>, the typed <see cref="VirtualView"/>
	/// and <c>PlatformView</c> members <b>throw <see cref="System.InvalidOperationException"/></b> rather
	/// than returning <see langword="null"/> once the handler is disconnected. Use the nullable
	/// <see cref="IElementHandler.VirtualView"/> and <see cref="IElementHandler.PlatformView"/> on paths
	/// that can run outside a connected window.
	/// </para>
	/// </remarks>
	/// <example>
	/// <code lang="csharp"><![CDATA[
	/// // Works for the in-box LabelHandler and for an external backend's label handler alike. The typed
	/// // members throw once the handler is disconnected, so guard on the nullable one first.
	/// if (label.Handler is { PlatformView: not null } handler &&
	///     handler is IViewHandler<ILabel, object> labelHandler)
	/// {
	///     ILabel virtualView = labelHandler.VirtualView;
	///     object nativeLabel = labelHandler.PlatformView;
	/// }
	/// ]]></code>
	/// </example>
	public interface IViewHandler<out TVirtualView, out TPlatformView> : IElementHandler<TVirtualView, TPlatformView>, IViewHandler
		where TVirtualView : IView
		where TPlatformView : class
	{
		/// <summary>
		/// Gets the cross-platform virtual view associated with the handler.
		/// </summary>
		/// <remarks>
		/// Re-declared so that lookups are unambiguous between
		/// <see cref="IElementHandler{TVirtualView, TPlatformView}.VirtualView"/> and
		/// <see cref="IViewHandler.VirtualView"/>. Throws
		/// <see cref="System.InvalidOperationException"/> when the handler is not connected; read the
		/// nullable <see cref="IViewHandler.VirtualView"/> instead on paths that can run while
		/// disconnected.
		/// </remarks>
		new TVirtualView VirtualView { get; }
	}
}
