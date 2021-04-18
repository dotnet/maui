using Gtk;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, CheckButton>
	{
		protected override CheckButton CreateNativeView()
		{
			return new CheckButton();
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateIsChecked(check);
		}
	}
}
