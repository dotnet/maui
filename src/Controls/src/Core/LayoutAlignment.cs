using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Values that represent LayoutAlignment.</summary>
	[Flags]
	public enum LayoutAlignment
	{
		/// <summary>The start of an alignment. Usually the Top or Left.</summary>
		Start = 0,
		/// <summary>The center of an alignment.</summary>
		Center = 1,
		/// <summary>The end of an alignment. Usually the Bottom or Right.</summary>
		End = 2,
		/// <summary>Fill the entire area if possible.</summary>
		Fill = 3
	}
}