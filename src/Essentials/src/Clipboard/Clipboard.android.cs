#nullable enable
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using static Android.Content.ClipboardManager;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ClipboardImplementation : IClipboard
	{
		static ClipboardManager? clipboardManager;

		static ClipboardManager? ClipboardManager =>
			clipboardManager ??= Application.Context.GetSystemService(Context.ClipboardService) as ClipboardManager;

		ClipboardChangeListener? clipboardListener;

		ClipboardChangeListener ClipboardListener =>
			clipboardListener ??= new ClipboardChangeListener(this);

		public Task SetTextAsync(string? text)
		{
			ClipboardManager?.PrimaryClip = ClipData.NewPlainText("Text", text ?? string.Empty);

			return Task.CompletedTask;
		}

		public bool HasText =>
			ClipboardManager is not null &&
			ClipboardManager.HasPrimaryClip &&
			!string.IsNullOrEmpty(ClipboardManager.PrimaryClip?.GetItemAt(0)?.Text);

		public Task<string?> GetTextAsync() =>
			Task.FromResult(ClipboardManager?.PrimaryClip?.GetItemAt(0)?.Text);

		void StartClipboardListeners()
			=> ClipboardManager?.AddPrimaryClipChangedListener(ClipboardListener);

		void StopClipboardListeners()
			=> ClipboardManager?.RemovePrimaryClipChangedListener(ClipboardListener);
	}

	class ClipboardChangeListener : Java.Lang.Object, IOnPrimaryClipChangedListener
	{
		ClipboardImplementation clipboard;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClipboardChangeListener"/> class.
		/// </summary>
		/// <param name="clipboard">An instance of <see cref="ClipboardImplementation"/> that will be used to listen for changes.</param>
		public ClipboardChangeListener(ClipboardImplementation clipboard)
		{
			this.clipboard = clipboard;
		}

		void IOnPrimaryClipChangedListener.OnPrimaryClipChanged() =>
			clipboard.OnClipboardContentChanged();
	}
}
