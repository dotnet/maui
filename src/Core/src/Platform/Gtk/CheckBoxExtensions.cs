using Gtk;

namespace Microsoft.Maui
{
	public static class CheckBoxExtensions
	{
		public static void UpdateIsChecked(this CheckButton platformView, ICheckBox checkBox)
		{
			platformView.Active = checkBox.IsChecked;
		}
	}
}