using System;

namespace System.Maui
{
	[Flags]
	public enum EffectiveFlowDirection
	{
		RightToLeft = 1 << 0,
		Explicit = 1 << 1
	}
}