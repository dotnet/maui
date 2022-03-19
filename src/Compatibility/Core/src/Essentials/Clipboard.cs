#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Clipboard']/Docs" />
	public static partial class Clipboard
	{
		static event EventHandler<EventArgs>? ClipboardContentChangedInternal;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='SetTextAsync']/Docs" />
		public static Task SetTextAsync(string? text)
			=> Current.SetTextAsync(text ?? string.Empty);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='HasText']/Docs" />
		public static bool HasText
			=> Current.HasText;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='GetTextAsync']/Docs" />
		public static Task<string?> GetTextAsync()
			=> Current.GetTextAsync();

		public static event EventHandler<EventArgs> ClipboardContentChanged
		{
			add
			{
				if (ClipboardContentChangedInternal == null)
					Current.ClipboardContentChanged += OnClipboardContentChanged;
				ClipboardContentChangedInternal += value;
			}
			remove
			{
				ClipboardContentChangedInternal -= value;
				if (ClipboardContentChangedInternal == null)
					Current.ClipboardContentChanged -= OnClipboardContentChanged;
			}
		}

		static IClipboard Current => ApplicationModel.DataTransfer.Clipboard.Default;

		static void OnClipboardContentChanged(object? sender, EventArgs e) =>
			ClipboardContentChangedInternal?.Invoke(sender, e);
	}
}
