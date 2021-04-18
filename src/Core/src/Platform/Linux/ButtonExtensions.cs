using Gtk;

namespace Microsoft.Maui
{
	public static class ButtonExtensions
	{
		public static void UpdateText(this Button nativeButton, IButton button)
		{
			nativeButton.Label = button.Text;
		}
	}
}