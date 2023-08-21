// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui;
using Microsoft.Maui.Handlers;

namespace Maui.Controls.Sample.Controls
{
	class BordelessEntryHandler : EntryHandler
	{
		public static PropertyMapper<BordelessEntry, BordelessEntryHandler> BorderlessEntryMapper = new PropertyMapper<BordelessEntry, BordelessEntryHandler>(EntryHandler.Mapper)
		{
			["Border"] = MapBorder
		};

		public BordelessEntryHandler()
			: base(BorderlessEntryMapper)
		{
		}

		public BordelessEntryHandler(PropertyMapper mapper = null)
			: base(mapper ?? BorderlessEntryMapper)
		{
		}

#if __ANDROID__
		public static void MapBorder(BordelessEntryHandler handler, BordelessEntry borderlessEntry)
		{
			handler.PlatformView.Background = null;
		}
#elif __IOS__
		public static void MapBorder(BordelessEntryHandler handler, BordelessEntry borderlessEntry)
		{
			handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
		}
#elif WINDOWS
		public static void MapBorder(BordelessEntryHandler handler, BordelessEntry borderlessEntry)
		{
		}
#elif TIZEN
		public static void MapBorder(BordelessEntryHandler handler, BordelessEntry borderlessEntry)
		{
		}
#else
		public static void MapBorder(BordelessEntryHandler handler, BordelessEntry borderlessEntry)
		{
		}
#endif
	}
}