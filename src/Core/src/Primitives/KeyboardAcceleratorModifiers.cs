using System;

namespace Microsoft.Maui
{
	[Flags]
	public enum KeyboardAcceleratorModifiers
	{
		None = 0,
		AlphaShift = 1 << 0, // CapsLock on MacCatalyst only. Currently not supported.
		Shift = 1 << 1, // Shift on MacCatalyst and windows
		Ctrl = 1 << 2, // Control on MacCatalyst and Windows
		Alt = 1 << 3, // Option on MacCatalyst, Menu on Windows
		Cmd = 1 << 4, // Command  on MacCatalyst
		Windows = 1 << 5, // Windows on Windows only
		NumericPad = 1 << 6 // NumericPad on MacCatalyst only. Currently not supported.// TODO: Test on catalyst
	}
}