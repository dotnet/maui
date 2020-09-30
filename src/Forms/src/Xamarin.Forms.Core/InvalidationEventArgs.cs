using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
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