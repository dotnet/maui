using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.DragStartingEventArgs']/Docs" />
	public class DragStartingEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public DragStartingEventArgs()
		{
			Data = new DataPackage();
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="//Member[@MemberName='Handled']/Docs" />
		public bool Handled { get; set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="//Member[@MemberName='Cancel']/Docs" />
		public bool Cancel { get; set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="//Member[@MemberName='Data']/Docs" />
		public DataPackage Data { get; private set; }
	}
}
