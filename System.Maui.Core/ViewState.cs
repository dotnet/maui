using System;

namespace System.Maui
{
	[Flags]
	public enum ViewState
	{
		Default = 0,
		Prelight = 1,
		Pressed = 1 << 1
	}
}