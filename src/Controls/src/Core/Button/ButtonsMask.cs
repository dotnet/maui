using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <summary>Flag values that represent mouse buttons.</summary>
	[Flags]
	public enum ButtonsMask
	{
		/// <summary>The primary (left) mouse button.</summary>
		Primary = 1 << 0,
		/// <summary>The secondary (right) mouse button.</summary>
		Secondary = 1 << 1
	}
}
