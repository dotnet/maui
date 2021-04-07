using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, CheckBox>
	{
		protected override CheckBox CreateNativeView() => new CheckBox();

		protected override void ConnectHandler(CheckBox nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.Checked += OnChecked;
			nativeView.Unchecked += OnChecked;
		}

		protected override void DisconnectHandler(CheckBox nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.Checked -= OnChecked;
			nativeView.Unchecked -= OnChecked;
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateIsChecked(check);
		}

		void OnChecked(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox nativeView && VirtualView != null)
				VirtualView.IsChecked = nativeView.IsChecked == true;
		}
	}
}