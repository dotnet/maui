using System;

namespace Microsoft.Maui
{
	[Flags]
	public enum EffectiveFlowDirection
	{
		RightToLeft = 1 << 0,
		Explicit = 1 << 1
	}
}