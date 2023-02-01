#nullable disable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapImeOptions(EntryHandler handler, Entry entry) =>
			MapImeOptions((IEntryHandler)handler, entry);

		public static void MapText(EntryHandler handler, Entry entry)
		{
			if (handler.DataFlowDirection == DataFlowDirection.FromPlatform)
			{
				Platform.EditTextExtensions.UpdateTextFromPlatform(handler.PlatformView, entry);
				return;
			}

			MapText((IEntryHandler)handler, entry);
		}

		public static void MapImeOptions(IEntryHandler handler, Entry entry)
		{
			Platform.EditTextExtensions.UpdateImeOptions(handler.PlatformView, entry);
		}

		public static void MapText(IEntryHandler handler, Entry entry)
		{
			Platform.EditTextExtensions.UpdateText(handler.PlatformView, entry);
		}
	}
}
