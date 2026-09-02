namespace Microsoft.Maui
{
	/// <summary>
	/// Defines the platform-neutral behavior contract for a handler that manages an <see cref="ILayout"/>.
	/// </summary>
	/// <typeparam name="TPlatformView">The native type this handler creates.</typeparam>
	/// <remarks>
	/// <para>
	/// <see cref="ILayoutHandler"/> declares the same child-management members, but it also declares
	/// <c>PlatformView</c> using a per-target-framework type alias, which prevents an external platform
	/// backend from implementing it. This interface carries the behavior without the alias.
	/// </para>
	/// <para>
	/// The two interfaces are intentionally unrelated: <see cref="ILayoutHandler"/> is untouched, so
	/// existing implementations and callers keep working. A handler that implements the members as
	/// ordinary public methods — as the in-box <see cref="Handlers.LayoutHandler"/> does — satisfies
	/// both interfaces at once.
	/// </para>
	/// <para>
	/// Because <typeparamref name="TPlatformView"/> is covariant, <c>ILayoutHandler&lt;object&gt;</c>
	/// matches any layout handler regardless of backend.
	/// </para>
	/// <para>
	/// <b>How .NET MAUI Controls reaches a layout handler.</b> <c>Microsoft.Maui.Controls.Layout</c> does
	/// not cast to either layout interface. It raises the child-management operations as <b>command
	/// mapper keys</b> — <c>Handler.Invoke(nameof(ILayoutHandler.Add), new LayoutHandlerUpdate(index,
	/// view))</c> and so on — which is type-agnostic and therefore reaches an external backend's handler
	/// exactly as it reaches the in-box one. Referencing <see cref="ILayoutHandler"/> for its member
	/// <em>names</em> is always allowed; only implementing it is blocked. An external backend should use
	/// those same key strings so Controls interop keeps working.
	/// </para>
	/// <para>
	/// <b>Known gap.</b> The obsolete
	/// <c>Microsoft.Maui.Controls.Compatibility.Layout&lt;T&gt;.LayoutHandler</c> property is declared as
	/// <see cref="ILayoutHandler"/> and evaluates <c>Handler as ILayoutHandler</c>, so it returns
	/// <see langword="null"/> for a handler that implements only this generic interface. It is the sole
	/// runtime cast to <see cref="ILayoutHandler"/> left in .NET MAUI, it is a public convenience
	/// property that no in-box code path reads, and its declaring type is <c>[Obsolete]</c> — so it does
	/// not affect layout behavior. It is called out here because this interface is, for that legacy
	/// property specifically, a compile-time contract only.
	/// </para>
	/// </remarks>
	public interface ILayoutHandler<out TPlatformView> : IViewHandler<ILayout, TPlatformView>
		where TPlatformView : class
	{
		/// <summary>
		/// Adds the specified child view to the end of the layout.
		/// </summary>
		/// <param name="view">The child view to add.</param>
		void Add(IView view);

		/// <summary>
		/// Removes the specified child view from the layout.
		/// </summary>
		/// <param name="view">The child view to remove.</param>
		void Remove(IView view);

		/// <summary>
		/// Removes all child views from the layout.
		/// </summary>
		void Clear();

		/// <summary>
		/// Inserts the specified child view into the layout at the given index.
		/// </summary>
		/// <param name="index">The index at which to insert the child view.</param>
		/// <param name="view">The child view to insert.</param>
		void Insert(int index, IView view);

		/// <summary>
		/// Replaces the child view at the given index with the specified child view.
		/// </summary>
		/// <param name="index">The index of the child view to replace.</param>
		/// <param name="view">The replacement child view.</param>
		void Update(int index, IView view);

		/// <summary>
		/// Updates the z-index ordering of the specified child view.
		/// </summary>
		/// <param name="view">The child view whose z-index changed.</param>
		void UpdateZIndex(IView view);
	}
}
