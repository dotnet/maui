using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, CheckBox>
	{
		protected override CheckBox CreatePlatformView() => new CheckBox();

		protected override void ConnectHandler(CheckBox platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Checked += OnChecked;
			platformView.Unchecked += OnChecked;
		}

		protected override void DisconnectHandler(CheckBox platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.Checked -= OnChecked;
			platformView.Unchecked -= OnChecked;
		}

		public static void MapIsChecked(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateIsChecked(check);
		}

		public static void MapForeground(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateForeground(check);
		}

		void OnChecked(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox platformView && VirtualView != null)
				VirtualView.IsChecked = platformView.IsChecked == true;
		}
	}
}