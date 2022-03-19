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

	public static partial class Clipboard
	{
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
