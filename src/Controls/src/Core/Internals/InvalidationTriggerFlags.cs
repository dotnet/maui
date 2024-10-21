using System;

namespace Microsoft.Maui.Controls.Internals;

[Flags]
internal enum InvalidationTriggerFlags : ushort
{
	None = 0,
	ApplyingBindingContext = 1 << 0,
	WillNotifyParentMeasureInvalidated = 1 << 1,
	WillTriggerHorizontalOptionsChanged = 1 << 2,
	WillTriggerVerticalOptionsChanged = 1 << 3,
	WillTriggerMarginChanged = 1 << 4,
	WillTriggerSizeRequestChanged = 1 << 5,
	WillTriggerMeasureChanged = 1 << 6,
	WillTriggerRendererReady = 1 << 7,
	WillTriggerUndefined = 1 << 8,
}

internal static class InvalidationTriggerFlagsExtensions {
	public static InvalidationTriggerFlags ToInvalidationTriggerFlags(this InvalidationTrigger trigger) {
		return trigger switch {
			InvalidationTrigger.MeasureChanged => InvalidationTriggerFlags.WillTriggerMeasureChanged,
			InvalidationTrigger.HorizontalOptionsChanged => InvalidationTriggerFlags.WillTriggerHorizontalOptionsChanged,
			InvalidationTrigger.VerticalOptionsChanged => InvalidationTriggerFlags.WillTriggerVerticalOptionsChanged,
			InvalidationTrigger.SizeRequestChanged => InvalidationTriggerFlags.WillTriggerSizeRequestChanged,
			InvalidationTrigger.RendererReady => InvalidationTriggerFlags.WillTriggerRendererReady,
			InvalidationTrigger.MarginChanged => InvalidationTriggerFlags.WillTriggerMarginChanged,
			_ => InvalidationTriggerFlags.WillTriggerUndefined,
		};
	}

	public static InvalidationTrigger ToInvalidationTrigger(this InvalidationTriggerFlags flags) {
		if ((flags & InvalidationTriggerFlags.WillTriggerUndefined) != 0) return InvalidationTrigger.Undefined;
		if ((flags & InvalidationTriggerFlags.WillTriggerRendererReady) != 0) return InvalidationTrigger.RendererReady;
		if ((flags & InvalidationTriggerFlags.WillTriggerMeasureChanged) != 0) return InvalidationTrigger.MeasureChanged;
		if ((flags & InvalidationTriggerFlags.WillTriggerSizeRequestChanged) != 0) return InvalidationTrigger.SizeRequestChanged;
		if ((flags & InvalidationTriggerFlags.WillTriggerMarginChanged) != 0) return InvalidationTrigger.MarginChanged;
		if ((flags & InvalidationTriggerFlags.WillTriggerVerticalOptionsChanged) != 0) return InvalidationTrigger.VerticalOptionsChanged;
		if ((flags & InvalidationTriggerFlags.WillTriggerHorizontalOptionsChanged) != 0) return InvalidationTrigger.HorizontalOptionsChanged;
		return InvalidationTrigger.Undefined;
	}
}