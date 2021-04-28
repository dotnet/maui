using Gtk;

namespace Microsoft.Maui
{

	public static class EntryExtensions
	{

		public static void UpdateText(this Entry nativeEntry, IEntry entry)
		{
			var text = entry.Text;

			if (nativeEntry.Text != text)
			{
				if (text != null)
					nativeEntry.Text = text;
				else
				{
					nativeEntry.Buffer.SetText(string.Empty, -1);

				}
			}

		}

	}

}