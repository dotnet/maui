using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class CheckBoxHandler : ViewHandler<ICheckBox, CheckBox>
	{
		protected override CheckBox CreatePlatformView()
		{
			var checkBox = new CheckBox();

			AdjustCheckBoxForNoText(checkBox);

			return checkBox;
		}

		static void AdjustCheckBoxForNoText(CheckBox checkBox)
		{
			checkBox.MinWidth = 0;
			checkBox.MinHeight = 0;
			checkBox.Padding = new UI.Xaml.Thickness(0);

			checkBox.Loaded += OnCheckBoxLoaded;

			static void OnCheckBoxLoaded(object sender, RoutedEventArgs e)
			{
				if (sender is not CheckBox checkBox)
					return;

				checkBox.Loaded -= OnCheckBoxLoaded;

				if (VisualTreeHelper.GetChildrenCount(checkBox) <= 0)
					return;

				var root = VisualTreeHelper.GetChild(checkBox, 0);
				if (root is not Grid rootGrid)
					return;

				var checkBoxHeight = Application.Current.Resources.TryGet<double>("CheckBoxHeight");
				var checkBoxSize = Application.Current.Resources.TryGet<double>("CheckBoxSize");
				var margin = (checkBoxHeight - checkBoxSize) / 2.0;

				rootGrid.Margin = new UI.Xaml.Thickness(margin);
			}
		}

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

		public static partial void MapIsChecked(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateIsChecked(check);
		}

		public static partial void MapForeground(ICheckBoxHandler handler, ICheckBox check)
		{
			handler.PlatformView?.UpdateForeground(check);
		}

		void OnChecked(object sender, RoutedEventArgs e)
		{
			if (sender is CheckBox platformView && VirtualView != null)
				VirtualView.IsChecked = platformView.IsChecked == true;
		}

		internal override bool PreventGestureBubbling => true;
	}
}