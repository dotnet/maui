// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class SwipeItemMenuItemHandler : ElementHandler<ISwipeItemMenuItem, object>
	{
		protected override object CreatePlatformElement()
		{
			throw new NotImplementedException();
		}

		public static void MapTextColor(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapCharacterSpacing(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapFont(ISwipeItemMenuItemHandler handler, ITextStyle view) { }

		public static void MapText(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapBackground(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		public static void MapVisibility(ISwipeItemMenuItemHandler handler, ISwipeItemMenuItem view) { }

		void IImageSourcePartSetter.SetImageSource(object? obj)
		{
			throw new NotImplementedException();
		}
	}
}
