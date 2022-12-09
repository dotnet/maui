using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapImeOptions(EntryHandler handler, Entry entry) =>
			MapImeOptions((IEntryHandler)handler, entry);

		public static void MapText(EntryHandler handler, Entry entry) =>
			MapText((IEntryHandler)handler, entry);

		public static void MapImeOptions(IEntryHandler handler, Entry entry)
		{
			Platform.EditTextExtensions.UpdateImeOptions(handler.PlatformView, entry);
		}

		public static void MapText(IEntryHandler handler, Entry entry)
		{
			Platform.EditTextExtensions.UpdateText(handler.PlatformView, entry);
		}

		static void MapFocus(IEntryHandler arg1, IEntry arg2, object arg3)
		{
			throw new NotImplementedException();
		}
	}
}
