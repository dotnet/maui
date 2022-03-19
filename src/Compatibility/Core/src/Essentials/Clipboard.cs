#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Clipboard']/Docs" />
	public static partial class Clipboard
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='SetTextAsync']/Docs" />
		public static Task SetTextAsync(string? text)
			=> Current.SetTextAsync(text ?? string.Empty);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='HasText']/Docs" />
		public static bool HasText
			=> Current.HasText;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='GetTextAsync']/Docs" />
		public static Task<string?> GetTextAsync()
			=> Current.GetTextAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='ClipboardContentChanged']/Docs" />
		public static event EventHandler<EventArgs> ClipboardContentChanged
		{
			add => Current.ClipboardContentChanged += value;
			remove => Current.ClipboardContentChanged -= value;
		}

		static IClipboard Current => ApplicationModel.DataTransfer.Clipboard.Default;
	}
}
