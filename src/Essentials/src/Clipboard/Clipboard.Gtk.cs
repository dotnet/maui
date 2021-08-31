using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{

	public static partial class Clipboard
	{

		static readonly Gdk.Atom clipboardAtom = Gdk.Atom.Intern("CLIPBOARD", false);

		static Task PlatformSetTextAsync(string text)
		{
			var clipboard = Gtk.Clipboard.Get(clipboardAtom);
			clipboard.Text = text;

			return Task.FromResult(0);
		}

		static bool PlatformHasText
		{
			get
			{
				var clipboard = Gtk.Clipboard.Get(clipboardAtom);

				return clipboard.WaitIsTextAvailable();
			}
		}

		static Task<string> PlatformGetTextAsync()
		{
			var clipboard = Gtk.Clipboard.Get(clipboardAtom);

			return Task.FromResult(clipboard.WaitForText());
		}

		static void StartClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		static void StopClipboardListeners()
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

	}

}