#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Clipboard']/Docs" />
	public static partial class Clipboard
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='SetTextAsync']/Docs" />
		[Obsolete($"Use {nameof(Clipboard)}.{nameof(Default)} instead.", true)]
		public static Task SetTextAsync(string? text)
			=> Default.SetTextAsync(text ?? string.Empty);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='HasText']/Docs" />
		[Obsolete($"Use {nameof(Clipboard)}.{nameof(Default)} instead.", true)]
		public static bool HasText
			=> Default.HasText;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='GetTextAsync']/Docs" />
		[Obsolete($"Use {nameof(Clipboard)}.{nameof(Default)} instead.", true)]
		public static Task<string?> GetTextAsync()
			=> Default.GetTextAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='ClipboardContentChanged']/Docs" />
		[Obsolete($"Use {nameof(Clipboard)}.{nameof(Default)} instead.", true)]
		public static event EventHandler<EventArgs> ClipboardContentChanged
		{
			add => Default.ClipboardContentChanged += value;
			remove => Default.ClipboardContentChanged -= value;
		}
	}
}
