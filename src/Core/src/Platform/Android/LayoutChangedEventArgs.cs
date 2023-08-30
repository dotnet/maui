using System;
using System.ComponentModel;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// This type is for internal use only by the .NET MAUI framework.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class LayoutChangedEventArgs : EventArgs
	{
		public LayoutChangedEventArgs()
		{

		}

		public LayoutChangedEventArgs(int l, int t, int r, int b)
		{
			Left = l;
			Top = t;
			Right = r;
			Bottom = b;
		}

		public int Left { get; set; }
		public int Top { get; set; }
		public int Right { get; set; }
		public int Bottom { get; set; }
	}
}
