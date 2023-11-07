#nullable enable

using System.Threading.Tasks;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{

	partial class ClipboardImplementation : IClipboard
	{

		static readonly Gdk.Atom clipboardAtom = Gdk.Atom.Intern("CLIPBOARD", false);

		public Task SetTextAsync(string? text)
		{
			var clipboard = Gtk.Clipboard.Get(clipboardAtom);
			clipboard.Text = text;

			return Task.FromResult(0);
		}

		public bool HasText
		{
			get
			{
				var clipboard = Gtk.Clipboard.Get(clipboardAtom);

				return clipboard.WaitIsTextAvailable();
			}
		}

		public Task<string?> GetTextAsync()
		{
			var clipboard = Gtk.Clipboard.Get(clipboardAtom);

			return Task.FromResult(clipboard.WaitForText())!;
		}

		void StartClipboardListeners()
		{
			throw ExceptionUtils.NotSupportedOrImplementedException;
		}

		void StopClipboardListeners()
		{
			throw ExceptionUtils.NotSupportedOrImplementedException;
		}

	}

}