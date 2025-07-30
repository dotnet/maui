#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>EventArgs for the NavigationPage's navigation events.</summary>
	public class NavigationEventArgs : EventArgs
	{
		/// <summary></summary>
		/// <param name="page">The page that was popped or is newly visible.</param>
		public NavigationEventArgs(Page page)
		{
			if (page == null)
				throw new ArgumentNullException(nameof(page));

			Page = page;
		}

		/// <summary>Gets the page that was removed or is newly visible.</summary>
		/// <remarks>For</remarks>
		public Page Page { get; private set; }
	}
}