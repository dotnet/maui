using System;

namespace Microsoft.Maui.Controls
{
	[Flags]
	public enum EffectiveFlowDirection
	{
		RightToLeft = 1 << 0,
		Explicit = 1 << 1
	}
}