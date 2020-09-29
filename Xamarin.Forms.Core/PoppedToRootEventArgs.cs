using System;
using System.Collections.Generic;

namespace Xamarin.Forms
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