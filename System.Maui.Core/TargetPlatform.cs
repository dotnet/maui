using System;
using System.ComponentModel;

namespace System.Maui
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum TargetPlatform
	{
		Other,
		iOS,
		Android,
		WinPhone,
		Windows
	}
}
