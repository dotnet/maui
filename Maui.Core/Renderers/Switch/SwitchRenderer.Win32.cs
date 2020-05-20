using System;
using WCheckBox = System.Windows.Controls.CheckBox;

namespace System.Maui.Platform
{
	public partial class SwitchRenderer : AbstractViewRenderer<ISwitch, WCheckBox>
	{
		protected override WCheckBox CreateView()
		{
			var checkBox = new WCheckBox();
			checkBox.Checked += OnChecked;
			checkBox.Unchecked += OnChecked;
			return checkBox;
		}

		public virtual void UpdateIsOn() => TypedNativeView.IsChecked = VirtualView.IsOn;

		public virtual void UpdateOnColor() { }

		public virtual void UpdateThumbColor() { }

		void OnChecked(object sender, System.Windows.RoutedEventArgs e)
		{
			VirtualView.IsOn = TypedNativeView.IsChecked.HasValue ? TypedNativeView.IsChecked.Value : false;
		}
	}
}
