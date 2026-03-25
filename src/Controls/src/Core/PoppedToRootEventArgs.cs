#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for a pop-to-root navigation operation.</summary>
	public class PoppedToRootEventArgs : NavigationEventArgs
	{
		/// <summary>Creates a new <see cref="PoppedToRootEventArgs"/>.</summary>
		/// <param name="page">The root page navigated to.</param>
		/// <param name="poppedPages">The pages that were popped.</param>
		public PoppedToRootEventArgs(Page page, IEnumerable<Page> poppedPages) : base(page)
		{
			if (poppedPages == null)
				throw new ArgumentNullException(nameof(poppedPages));

			PoppedPages = poppedPages;
		}

		/// <summary>Gets the pages that were popped from the navigation stack.</summary>
		public IEnumerable<Page> PoppedPages { get; private set; }
	}
}
