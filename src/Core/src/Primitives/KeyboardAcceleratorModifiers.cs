using System;

namespace Microsoft.Maui
{
	/// <summary>
	/// Enumerates modifier flags for keyboard accelerators. MacCatalyst modifiers AlphaShift and NumericPad are not currently supported. 
	/// </summary>
	[Flags]
	public enum KeyboardAcceleratorModifiers
	{
		/// <summary>
		/// Indicates no modifier.
		/// </summary>
		None = 0,
		/// <summary>
		/// Indicates Shift modifier on both MacCatalyst and Windows.
		/// </summary>
		Shift = 1 << 0,
		/// <summary>
		/// Indicates Control modifier on both MacCatalyst and Windows.
		/// </summary>
		Ctrl = 1 << 1,
		/// <summary>
		/// Indicates Option modifier on MacCatalyst, Menu modifier on Windows.
		/// </summary>
		Alt = 1 << 2,
		/// <summary>
		/// Indicates Command modifier on MacCatalyst only.
		/// </summary>
		Cmd = 1 << 3,
		/// <summary>
		/// Indicates Windows modifier on Windows only.
		/// </summary>
		Windows = 1 << 4
	}
}