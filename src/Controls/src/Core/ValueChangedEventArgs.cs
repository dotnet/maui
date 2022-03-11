using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ValueChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ValueChangedEventArgs']/Docs" />
	public class ValueChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ValueChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ValueChangedEventArgs(double oldValue, double newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ValueChangedEventArgs.xml" path="//Member[@MemberName='NewValue']/Docs" />
		public double NewValue { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ValueChangedEventArgs.xml" path="//Member[@MemberName='OldValue']/Docs" />
		public double OldValue { get; private set; }
	}
}