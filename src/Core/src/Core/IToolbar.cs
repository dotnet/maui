namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a bar that may display the page title, navigation affordances, and other interactive items.
	/// </summary>
	public interface IToolbar : IElement
	{
		/// <summary>
		/// Gets or sets a value that indicates whether the back button is enabled or disabled.
		/// </summary>
		bool BackButtonVisible { get; set; }

		/// <summary>
		///  Gets or sets a value that indicates whether the toolbar is visible or not.
		/// </summary>
		bool IsVisible { get; set; }

		/// <summary>
		/// Gets the title for the Toolbar.
		/// </summary>
		string Title { get; }
	}

	/// <summary>
	/// Provides the visibility of the drawer (flyout) toggle affordance for an <see cref="IToolbar"/>,
	/// commonly rendered as a "hamburger" button in the toolbar's navigation slot.
	/// </summary>
	/// <remarks>
	/// <para>This is a separate optional interface to preserve compatibility for existing
	/// <see cref="IToolbar"/> implementations, including netstandard2.0 targets where adding the member
	/// to the existing interface would require implementers to add it.</para>
	/// <para>Consume it by pattern matching on an <see cref="IToolbar"/>:
	/// <c>if (toolbar is IToolbarDrawerToggleVisible { DrawerToggleVisible: true })</c>. A toolbar that
	/// does not implement this interface has no drawer toggle.</para>
	/// </remarks>
	public interface IToolbarDrawerToggleVisible
	{
		/// <summary>
		/// Gets a value that indicates whether the drawer (flyout) toggle affordance should be displayed
		/// in the navigation slot of the toolbar.
		/// </summary>
		/// <remarks>
		/// <para>The value is computed and owned by the cross-platform layer (for example <c>Shell</c>, or a
		/// <c>NavigationPage</c> hosted inside a <c>FlyoutPage</c>) from the current flyout behavior and
		/// navigation stack, which is why this contract is read-only. Platform backends should render it,
		/// not compute or overwrite it.</para>
		/// <para>This property and <see cref="IToolbar.BackButtonVisible"/> are not mutually exclusive:
		/// on Windows both can be <see langword="true"/> at the same time. They share a single navigation
		/// slot and <see cref="IToolbar.BackButtonVisible"/> takes precedence when rendering, so a backend
		/// should check the back button first and only fall through to the drawer toggle.</para>
		/// <para>Changes raise a handler update keyed on <c>"DrawerToggleVisible"</c>, so a backend maps it
		/// like any other toolbar property.</para>
		/// </remarks>
		bool DrawerToggleVisible { get; }
	}
}
