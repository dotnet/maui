using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	internal class InvalidationEventArgs : EventArgs
	{
		static InvalidationEventArgs? s_undefined;
		static InvalidationEventArgs? s_measureChanged;
		static InvalidationEventArgs? s_horizontalOptionsChanged;
		static InvalidationEventArgs? s_verticalOptionsChanged;
		static InvalidationEventArgs? s_sizeRequestChanged;
		static InvalidationEventArgs? s_rendererReady;
		static InvalidationEventArgs? s_marginChanged;

		public InvalidationEventArgs(InvalidationTrigger trigger)
		{
			Trigger = trigger;
		}

		public InvalidationTrigger Trigger { get; private set; }

		internal static InvalidationEventArgs GetCached(InvalidationTrigger trigger) => trigger switch
		{
			InvalidationTrigger.Undefined => s_undefined ??= new InvalidationEventArgs(trigger),
			InvalidationTrigger.MeasureChanged => s_measureChanged ??= new InvalidationEventArgs(trigger),
			InvalidationTrigger.HorizontalOptionsChanged => s_horizontalOptionsChanged ??= new InvalidationEventArgs(trigger),
			InvalidationTrigger.VerticalOptionsChanged => s_verticalOptionsChanged ??= new InvalidationEventArgs(trigger),
			InvalidationTrigger.SizeRequestChanged => s_sizeRequestChanged ??= new InvalidationEventArgs(trigger),
			InvalidationTrigger.RendererReady => s_rendererReady ??= new InvalidationEventArgs(trigger),
			InvalidationTrigger.MarginChanged => s_marginChanged ??= new InvalidationEventArgs(trigger),
			_ => new InvalidationEventArgs(trigger),
		};
	}
}