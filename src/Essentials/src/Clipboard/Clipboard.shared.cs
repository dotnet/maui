#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	public interface IClipboard
	{
		bool HasText { get; }

		Task SetTextAsync(string? text);

		Task<string?> GetTextAsync();

		event EventHandler<EventArgs> ClipboardContentChanged;
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="Type[@FullName='Microsoft.Maui.Essentials.Clipboard']/Docs" />
	public static class Clipboard
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='SetTextAsync']/Docs" />
		public static Task SetTextAsync(string? text)
			=> Default.SetTextAsync(text ?? string.Empty);

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='HasText']/Docs" />
		public static bool HasText
			=> Default.HasText;

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='GetTextAsync']/Docs" />
		public static Task<string?> GetTextAsync()
			=> Default.GetTextAsync();

		/// <include file="../../docs/Microsoft.Maui.Essentials/Clipboard.xml" path="//Member[@MemberName='ClipboardContentChanged']/Docs" />
		public static event EventHandler<EventArgs> ClipboardContentChanged
		{
			add => Default.ClipboardContentChanged += value;
			remove => Default.ClipboardContentChanged -= value;
		}

		static IClipboard? defaultImplementation;

		public static IClipboard Default =>
			defaultImplementation ??= new ClipboardImplementation();

		internal static void SetDefault(IClipboard? implementation) =>
			defaultImplementation = implementation;
	}

	partial class ClipboardImplementation : IClipboard
	{
		event EventHandler<EventArgs>? ClipboardContentChangedInternal;

		public event EventHandler<EventArgs> ClipboardContentChanged
		{
			add
			{
				if (ClipboardContentChangedInternal == null)
					StartClipboardListeners();
				ClipboardContentChangedInternal += value;
			}
			remove
			{
				ClipboardContentChangedInternal -= value;
				if (ClipboardContentChangedInternal == null)
					StopClipboardListeners();
			}
		}

		internal void OnClipboardContentChanged() =>
			ClipboardContentChangedInternal?.Invoke(this, EventArgs.Empty);
	}
}
