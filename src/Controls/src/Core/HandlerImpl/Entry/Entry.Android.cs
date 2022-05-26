using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapImeOptions(EntryHandler handler, Entry entry)
		{
			Platform.EditTextExtensions.UpdateImeOptions(handler.PlatformView, entry);
		}

		public static void MapText(EntryHandler handler, Entry entry)
		{
			Platform.EditTextExtensions.UpdateText(handler.PlatformView, entry);
		}
	}
}
