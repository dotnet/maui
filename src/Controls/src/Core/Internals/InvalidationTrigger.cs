using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	[Flags]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum InvalidationTrigger
	{
		Undefined = 0,
		MeasureChanged = 1 << 0,
		HorizontalOptionsChanged = 1 << 1,
		VerticalOptionsChanged = 1 << 2,
		SizeRequestChanged = 1 << 3,
		RendererReady = 1 << 4,
		MarginChanged = 1 << 5
	}
}