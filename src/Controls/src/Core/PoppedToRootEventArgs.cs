using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
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
