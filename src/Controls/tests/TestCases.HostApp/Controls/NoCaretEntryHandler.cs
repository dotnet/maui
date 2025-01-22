using System;
using System.Collections.Generic;

namespace Maui.Controls.Sample
{
	public class NoCaretEntryHandler
	{
		public static void RemoveCaret()
		{
			Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(NoCaretEntry), (handler, view) =>
			{
				if (view is NoCaretEntry)
				{
#if ANDROID
					handler.PlatformView.SetCursorVisible(false);
#elif IOS
					handler.PlatformView.TintColor = UIKit.UIColor.Clear;
#endif
				}
			});
		}
	}
}