using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/PositionChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.PositionChangedEventArgs']/Docs" />
	public class PositionChangedEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/PositionChangedEventArgs.xml" path="//Member[@MemberName='PreviousPosition']/Docs" />
		public int PreviousPosition { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/PositionChangedEventArgs.xml" path="//Member[@MemberName='CurrentPosition']/Docs" />
		public int CurrentPosition { get; }

		internal PositionChangedEventArgs(int previousPosition, int currentPosition)
		{
			PreviousPosition = previousPosition;
			CurrentPosition = currentPosition;
		}
	}
}
