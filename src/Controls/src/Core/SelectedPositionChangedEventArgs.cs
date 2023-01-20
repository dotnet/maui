#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SelectedPositionChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.SelectedPositionChangedEventArgs']/Docs/*" />
	public class SelectedPositionChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SelectedPositionChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public SelectedPositionChangedEventArgs(int selectedPosition)
		{
			SelectedPosition = selectedPosition;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SelectedPositionChangedEventArgs.xml" path="//Member[@MemberName='SelectedPosition']/Docs/*" />
		public object SelectedPosition { get; private set; }
	}
}