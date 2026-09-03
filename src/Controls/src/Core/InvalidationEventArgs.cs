#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	internal class InvalidationEventArgs : EventArgs
	{
		public InvalidationEventArgs(InvalidationTrigger trigger)
		{
			Trigger = trigger;
		}

		public InvalidationTrigger Trigger { get; private set; }
	}
}