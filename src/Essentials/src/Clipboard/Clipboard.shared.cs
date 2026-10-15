#nullable enable
using System;
using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	/// <summary>
	/// Provides a way to work with text on the device clipboard.
	/// </summary>
	public interface IClipboard
	{
		/// <summary>
		/// Gets a value indicating whether there is any text on the clipboard.
		/// </summary>
		bool HasText { [Microsoft.Maui.Interop.HybridAction("clipboard.hasText", MainThread = true)] get; }

		/// <summary>
		/// Sets the contents of the clipboard to be the specified text.
		/// </summary>
		/// <param name="text">The text to put on the clipboard.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		/// <remarks>This method returns immediately and does not guarantee that the text is on the clipboard by the time this method returns.</remarks>
		[Microsoft.Maui.Interop.HybridAction("clipboard.write", MainThread = true)]
		Task SetTextAsync(string? text);

		/// <summary>
		/// Returns any text that is on the clipboard.
		/// </summary>
		/// <returns>Text content that is on the clipboard, or <see langword="null"/> if there is none.</returns>
		[Microsoft.Maui.Interop.HybridAction("clipboard.read", MainThread = true)]
		Task<string?> GetTextAsync();

		/// <summary>
		/// Occurs when the clipboard content changes.
		/// </summary>
		[Microsoft.Maui.Interop.HybridEvent("clipboard.changed")]
		event EventHandler<EventArgs> ClipboardContentChanged;
	}

	/// <summary>
	/// Provides a way to work with text on the device clipboard.
	/// </summary>
	public static class Clipboard
	{
		/// <summary>
		/// Sets the contents of the clipboard to be the specified text.
		/// </summary>
		/// <param name="text">The text to put on the clipboard.</param>
		/// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
		/// <remarks>This method returns immediately and does not guarantee that the text is on the clipboard by the time this method returns.</remarks>
		public static Task SetTextAsync(string? text)
			=> Default.SetTextAsync(text ?? string.Empty);

		/// <summary>
		/// Gets a value indicating whether there is any text on the clipboard.
		/// </summary>
		public static bool HasText
			=> Default.HasText;

		/// <summary>
		/// Returns any text that is on the clipboard.
		/// </summary>
		/// <returns>Text content that is on the clipboard, or <see langword="null"/> if there is none.</returns>
		public static Task<string?> GetTextAsync()
			=> Default.GetTextAsync();

		/// <summary>
		/// Occurs when the clipboard content changes.
		/// </summary>
		public static event EventHandler<EventArgs> ClipboardContentChanged
		{
			add => Default.ClipboardContentChanged += value;
			remove => Default.ClipboardContentChanged -= value;
		}

		static IClipboard? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IClipboard Default =>
			EssentialsImplementation.GetOrCreate(ref defaultImplementation, static () => new ClipboardImplementation());

		internal static void SetDefault(IClipboard? implementation) =>
			EssentialsImplementation.Set(ref defaultImplementation, implementation);
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
