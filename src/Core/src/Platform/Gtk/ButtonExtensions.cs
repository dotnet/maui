using Gtk;

namespace Microsoft.Maui
{

	public static class ButtonExtensions
	{

		public static void UpdateText(this Button nativeButton, IButton button)
		{
			// need to attach Attributes after setting text again, so get it ...
			var attrs = (nativeButton.Child as Label)?.Attributes;

			// cause maybe a new label is created on assiging text:
			nativeButton.Label = button.Text;

			if (nativeButton.Child is Label lbl)
			{
				// and set it again on the new label:
				lbl.Attributes = attrs;
			}
		}

	}

}