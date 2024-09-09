#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		/// <summary>
		/// Maps the abstract InputTransparent property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="Layout"/> instance.</param>
		[Obsolete]
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) { }

		/// <summary>
		/// Maps the abstract InputTransparent property to the platform-specific implementations.
		/// </summary>
		/// <param name="handler">The associated handler.</param>
		/// <param name="layout">The associated <see cref="Layout"/> instance.</param>
		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) { }
	}
}
