using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, MauiCheckBox>
	{
		protected override MauiCheckBox CreateNativeView() => new MauiCheckBox();

		protected override void ConnectHandler(MauiCheckBox nativeView)
		{
			base.ConnectHandler(nativeView);

			nativeView.Checked += OnChecked;
			nativeView.Unchecked += OnChecked;
		}

		protected override void DisconnectHandler(MauiCheckBox nativeView)
		{
			base.DisconnectHandler(nativeView);

			nativeView.Checked -= OnChecked;
			nativeView.Unchecked -= OnChecked;
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateIsChecked(check);
		}

		public static void MapForeground(CheckBoxHandler handler, ICheckBox check)
		{
			handler.NativeView?.UpdateForeground(check);
		}

		void OnChecked(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox nativeView && VirtualView != null)
				VirtualView.IsChecked = nativeView.IsChecked == true;
		}
	}
}