// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler : ViewHandler<IIndicatorView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static void MapCount(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapPosition(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapHideSingle(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapMaximumVisible(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorSize(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapSelectedIndicatorColor(IIndicatorViewHandler handler, IIndicatorView indicator) { }
		public static void MapIndicatorShape(IIndicatorViewHandler handler, IIndicatorView indicator) { }
	}
}
