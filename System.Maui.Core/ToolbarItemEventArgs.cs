using System;

namespace Xamarin.Forms
{
	internal class ToolbarItemEventArgs : EventArgs
	{
		public ToolbarItemEventArgs(ToolbarItem item)
		{
			ToolbarItem = item;
		}

		public ToolbarItem ToolbarItem { get; private set; }
	}
}