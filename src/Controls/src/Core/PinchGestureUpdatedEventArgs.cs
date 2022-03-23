using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureUpdatedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.PinchGestureUpdatedEventArgs']/Docs" />
	public class PinchGestureUpdatedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureUpdatedEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public PinchGestureUpdatedEventArgs(GestureStatus status, double scale, Point origin) : this(status)
		{
			ScaleOrigin = origin;
			Scale = scale;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureUpdatedEventArgs.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public PinchGestureUpdatedEventArgs(GestureStatus status)
		{
			Status = status;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureUpdatedEventArgs.xml" path="//Member[@MemberName='Scale']/Docs" />
		public double Scale { get; } = 1;

		/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureUpdatedEventArgs.xml" path="//Member[@MemberName='ScaleOrigin']/Docs" />
		public Point ScaleOrigin { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/PinchGestureUpdatedEventArgs.xml" path="//Member[@MemberName='Status']/Docs" />
		public GestureStatus Status { get; }
	}
}