using System;

namespace System.Maui
{
	public class NavigationEventArgs : EventArgs
	{
		public NavigationEventArgs(Page page)
		{
			if (page == null)
				throw new ArgumentNullException("page");

			Page = page;
		}

		public Page Page { get; private set; }
	}
}