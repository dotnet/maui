using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Entry
	{
		public static void MapText(EntryHandler handler, Entry entry)
		{
			Platform.EditTextExtensions.UpdateText(handler.PlatformView, entry);
		}
	}
}
