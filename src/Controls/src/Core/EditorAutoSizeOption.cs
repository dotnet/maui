using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	/// <summary>Enumerates values that control whether an editor will change size to accommodate input as the user enters it.</summary>
	public enum EditorAutoSizeOption
	{
		/// <summary>Automatic resizing is not enabled. This is the default value.</summary>
		Disabled = 0,
		/// <summary>Automatic resizing is enabled.</summary>
		TextChanges = 1
	}
}
