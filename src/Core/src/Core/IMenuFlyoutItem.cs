using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a command in a MenuFlyout view.
	/// </summary>
	public interface IMenuFlyoutItem : IMenuElement
	{
		/// <summary>
		/// Represents a shortcut key for a MenuItem.
		/// </summary>
#if NETSTANDARD2_0
		IReadOnlyList<IKeyboardAccelerator>? KeyboardAccelerators { get; }
#else
		IReadOnlyList<IKeyboardAccelerator>? KeyboardAccelerators => null;
#endif
	}
}
