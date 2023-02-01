#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ScrolledEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ScrolledEventArgs']/Docs/*" />
	public class ScrolledEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ScrolledEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ScrolledEventArgs(double x, double y)
		{
			ScrollX = x;
			ScrollY = y;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrolledEventArgs.xml" path="//Member[@MemberName='ScrollX']/Docs/*" />
		public double ScrollX { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ScrolledEventArgs.xml" path="//Member[@MemberName='ScrollY']/Docs/*" />
		public double ScrollY { get; private set; }
	}
}