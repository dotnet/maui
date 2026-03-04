namespace Microsoft.Maui;

/// <summary>
/// Enumeration of modes for the root menu of a Shell application.
/// </summary>
public enum FlyoutBehavior
{
	/// <summary>
	/// The flyout menu is disabled.
	/// </summary>
	Disabled,

	/// <summary>
	/// The flyout menu can be opened or closed by the user.
	/// </summary>
	Flyout,

	/// <summary>
	/// The flyout menu is locked open and cannot be closed by the user.
	/// </summary>
	Locked
}
