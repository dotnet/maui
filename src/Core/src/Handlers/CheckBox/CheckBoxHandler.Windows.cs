using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, MauiCheckBox>
	{
		protected override MauiCheckBox CreatePlatformView() => new MauiCheckBox();

		protected override void ConnectHandler(MauiCheckBox platformView)
		{
			base.ConnectHandler(platformView);

			platformView.Checked += OnChecked;
			platformView.Unchecked += OnChecked;
		}

		protected override void DisconnectHandler(MauiCheckBox platformView)
		{
			base.DisconnectHandler(platformView);

			platformView.Checked -= OnChecked;
			platformView.Unchecked -= OnChecked;
		}

		public static void MapIsChecked(CheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateIsChecked(check);
		}

		public static void MapForeground(CheckBoxHandler handler, ICheckBox check)
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