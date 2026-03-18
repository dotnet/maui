#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;

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
			if (handler is ViewHandler viewHandler && viewHandler.DataFlowDirection == DataFlowDirection.FromPlatform)
			{
				Platform.EditTextExtensions.UpdateTextFromPlatform(handler.PlatformView, entry);
				return;
			}

			Platform.EditTextExtensions.UpdateText(handler.PlatformView, entry);
		}

		// TODO: Material3: Make it public in .NET 11
		// MaterialEntryHandler-specific overloads
		internal static void MapImeOptions(EntryHandler2 handler, Entry entry)
		{
			if (handler.PlatformView?.EditText is null)
			{
				return;
			}

			Platform.EditTextExtensions.UpdateImeOptions(handler.PlatformView.EditText, entry);
		}

		// TODO: Material3: Make it public in .NET 11
		internal static void MapText(EntryHandler2 handler, Entry entry)
		{
			if (handler.PlatformView?.EditText is null)
			{
				return;
			}

			if (handler.DataFlowDirection == DataFlowDirection.FromPlatform)
			{
				Platform.EditTextExtensions.UpdateTextFromPlatform(handler.PlatformView.EditText, entry);
				return;
			}

			Platform.EditTextExtensions.UpdateText(handler.PlatformView.EditText, entry);
		}
	}
}
