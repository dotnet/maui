using System;
using ERect = ElmSharp.Rect;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Holds information about size of the area which can be used for layout.
	/// </summary>
	public class LayoutEventArgs : EventArgs
	{
		/// <summary>
		/// Geometry of the layout area, absolute coordinate
		/// </summary>
		public ERect Geometry
		{
			get;
			internal set;
		}
	}
}
