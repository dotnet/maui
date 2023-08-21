// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public partial class Layout
	{
		[Obsolete]
		public static void MapInputTransparent(LayoutHandler handler, Layout layout) { }

		[Obsolete]
		public static void MapInputTransparent(ILayoutHandler handler, Layout layout) { }

		[Obsolete]
		static void MapInputTransparent(IViewHandler handler, IView layout) { }
	}
}
