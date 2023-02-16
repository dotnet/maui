#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/PanUpdatedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.PanUpdatedEventArgs']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/PanUpdatedEventArgs.xml" path="//Member[@MemberName='GestureId']/Docs/*" />
		public int GestureId { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/PanUpdatedEventArgs.xml" path="//Member[@MemberName='StatusType']/Docs/*" />
		public GestureStatus StatusType { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/PanUpdatedEventArgs.xml" path="//Member[@MemberName='TotalX']/Docs/*" />
		public double TotalX { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/PanUpdatedEventArgs.xml" path="//Member[@MemberName='TotalY']/Docs/*" />
		public double TotalY { get; }
	}
}