#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.DragStartingEventArgs']/Docs/*" />
	public class DragStartingEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DragStartingEventArgs()
		{
			Data = new DataPackage();
		}

		public DragStartingEventArgs(Point position)
		{
			Data = new DataPackage();
			Position = position;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="//Member[@MemberName='Handled']/Docs/*" />
		public bool Handled { get; set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="//Member[@MemberName='Cancel']/Docs/*" />
		public bool Cancel { get; set; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/DragStartingEventArgs.xml" path="//Member[@MemberName='Data']/Docs/*" />
		public DataPackage Data { get; private set; }

		/// <summary>
		/// A point in the coordinate system that is where is dragging started.
		/// </summary>
		public Point Position { get; set; }
	}
}
