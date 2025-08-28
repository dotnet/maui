#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event that is raised when a pan gesture updates.</summary>
	public class PanUpdatedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/PanUpdatedEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public PanUpdatedEventArgs(GestureStatus type, int gestureId, double totalx, double totaly) : this(type, gestureId)
		{
			TotalX = totalx;
			TotalY = totaly;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/PanUpdatedEventArgs.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public PanUpdatedEventArgs(GestureStatus type, int gestureId)
		{
			StatusType = type;
			GestureId = gestureId;
		}

		/// <summary>Gets the identifier for the gesture that raised the event.</summary>
		public int GestureId { get; }

		/// <summary>Gets a value that tells if this event is for a newly started gesture, a running gesture, a completed gesture, or a canceled gesture.</summary>
		public GestureStatus StatusType { get; }

		/// <summary>Gets the total change in the X direction since the beginning of the gesture.</summary>
		public double TotalX { get; }

		/// <summary>Gets the total change in the Y direction since the beginning of the gesture.</summary>
		public double TotalY { get; }
	}
}