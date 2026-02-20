using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Flags indicating which property changes should trigger layout invalidation.</summary>
	[Flags]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum InvalidationTrigger
	{
		/// <summary>No invalidation trigger specified.</summary>
		Undefined = 0,
		/// <summary>Invalidation due to a change in measured size.</summary>
		MeasureChanged = 1 << 0,
		/// <summary>Invalidation due to a change in horizontal layout options.</summary>
		HorizontalOptionsChanged = 1 << 1,
		/// <summary>Invalidation due to a change in vertical layout options.</summary>
		VerticalOptionsChanged = 1 << 2,
		/// <summary>Invalidation due to a change in requested size.</summary>
		SizeRequestChanged = 1 << 3,
		/// <summary>Invalidation due to the renderer becoming ready.</summary>
		RendererReady = 1 << 4,
		/// <summary>Invalidation due to a change in margin.</summary>
		MarginChanged = 1 << 5
	}
}