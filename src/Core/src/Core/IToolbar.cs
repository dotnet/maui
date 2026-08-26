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
		/// Gets a value that indicates whether the drawer (flyout) toggle affordance, commonly rendered as a
		/// "hamburger" button, should be displayed in the navigation slot of the toolbar.
		/// </summary>
		/// <remarks>
		/// <para>The value is computed and owned by the cross-platform layer (for example <c>Shell</c> or a
		/// <c>NavigationPage</c> hosted inside a <c>FlyoutPage</c>) based on the current flyout behavior and
		/// navigation stack, so it is read-only on this contract.</para>
		/// <para><see cref="BackButtonVisible"/> takes precedence: when the back button is visible the navigation
		/// slot renders the back affordance even if this property is <see langword="true"/>.</para>
		/// <para>Changes to this value raise a handler update for the <c>DrawerToggleVisible</c> property name, so
		/// platform backends can map it just like any other toolbar property.</para>
		/// <para>The default implementation returns <see langword="false"/> so that existing
		/// <see cref="IToolbar"/> implementations remain source and binary compatible.</para>
		/// </remarks>
#if NETSTANDARD2_0
		bool DrawerToggleVisible { get; }
#else
		bool DrawerToggleVisible => false;
#endif

		/// <summary>
		/// Gets the title for the Toolbar.
		/// </summary>
		string Title { get; }
	}
}
