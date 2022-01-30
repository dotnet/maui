using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Clipboard']/Docs" />
	public static partial class Clipboard
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='SetTextAsync']/Docs" />
		public static Task SetTextAsync(string text)
			=> PlatformSetTextAsync(text ?? string.Empty);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='HasText']/Docs" />
		public static bool HasText
			=> PlatformHasText;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='GetTextAsync']/Docs" />
		public static Task<string> GetTextAsync()
			=> PlatformGetTextAsync();

		public static event EventHandler<EventArgs> ClipboardContentChanged
		{
			add
			{
				var wasRunning = ClipboardContentChangedInternal != null;

				ClipboardContentChangedInternal += value;

				if (!wasRunning && ClipboardContentChangedInternal != null)
				{
					StartClipboardListeners();
				}
			}

			remove
			{
				var wasRunning = ClipboardContentChangedInternal != null;

				ClipboardContentChangedInternal -= value;

				if (wasRunning && ClipboardContentChangedInternal == null)
					StopClipboardListeners();
			}
		}

		static event EventHandler<EventArgs> ClipboardContentChangedInternal;

		internal static void ClipboardChangedInternal() => ClipboardContentChangedInternal?.Invoke(null, EventArgs.Empty);
	}
}
