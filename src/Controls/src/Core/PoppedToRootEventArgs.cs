using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/PoppedToRootEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.PoppedToRootEventArgs']/Docs" />
	public class PoppedToRootEventArgs : NavigationEventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/PoppedToRootEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public PoppedToRootEventArgs(Page page, IEnumerable<Page> poppedPages) : base(page)
		{
			if (poppedPages == null)
				throw new ArgumentNullException(nameof(poppedPages));

			PoppedPages = poppedPages;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/PoppedToRootEventArgs.xml" path="//Member[@MemberName='PoppedPages']/Docs" />
		public IEnumerable<Page> PoppedPages { get; private set; }
	}
}
