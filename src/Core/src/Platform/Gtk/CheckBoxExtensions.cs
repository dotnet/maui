using Gtk;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this CheckButton nativeCheckBox, ICheckBox checkBox)
		{
			nativeCheckBox.Active = checkBox.IsChecked;
		}
	}
}