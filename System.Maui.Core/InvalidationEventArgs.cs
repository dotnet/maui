using System;
using System.Maui.Internals;

namespace System.Maui
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