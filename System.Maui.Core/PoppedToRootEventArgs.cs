using System;
using System.Collections.Generic;

namespace System.Maui
{
	public class PoppedToRootEventArgs : NavigationEventArgs
	{
		public PoppedToRootEventArgs(Page page, IEnumerable<Page> poppedPages) : base(page)
		{
			if (poppedPages == null)
				throw new ArgumentNullException(nameof(poppedPages));

			PoppedPages = poppedPages;
		}

		public IEnumerable<Page> PoppedPages { get; private set; }
	}
}
