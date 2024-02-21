using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a shortcut key for a <see cref="T:Microsoft.Maui.Controls.MenuFlyoutItem" />.
	/// </summary>
	public interface IKeyboardAccelerator
	{
		/// <summary>
		/// Specifies the virtual key used to modify another keypress. 
		/// </summary>
		public KeyboardAcceleratorModifiers Modifiers { get; }

		/// <summary>
		/// Specifies the values for the virtual key.
		/// </summary>
		public string? Key { get; }
	}
}
