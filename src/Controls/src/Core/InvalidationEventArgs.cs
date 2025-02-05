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
		public InvalidationEventArgs(InvalidationTrigger trigger, int depth) : this(trigger)
		{
			CurrentInvalidationDepth = depth;
		}


		public InvalidationTrigger Trigger { get; private set; }


		public int CurrentInvalidationDepth { set; get; }
	}
}