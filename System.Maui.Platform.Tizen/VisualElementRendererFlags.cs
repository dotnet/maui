using System;

namespace System.Maui.Platform.Tizen
{
	[Flags]
	public enum VisualElementRendererFlags : byte
	{
		None = 0,
		Disposed = 1,
		NeedsLayout = 2,
		NeedsTransformation = 4,
	}
}
