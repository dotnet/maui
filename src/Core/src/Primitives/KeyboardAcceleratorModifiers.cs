using System;

namespace Microsoft.Maui
{
	[Flags]
	public enum KeyboardAcceleratorModifiers
	{
		None = 0,
		Shift = 1 << 0,     // Shift on MacCatalyst and Windows
		Ctrl = 1 << 1,      // Control on MacCatalyst and Windows
		Alt = 1 << 2,       // Option on MacCatalyst, Menu on Windows
		Cmd = 1 << 3,		// Command  on MacCatalyst
		Windows = 1 << 4	// Windows on Windows only
		// AlphaShift		// CapsLock on MacCatalyst only. Currently not supported.
		// NumericPad		// NumericPad on MacCatalyst only. Currently not supported.
	}

}