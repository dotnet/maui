using System.Maui.Core.Controls;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Maui.Platform
{
	public partial class CheckBoxRenderer
	{
		static Brush _tintDefaultBrush = Color.Transparent.ToBrush();

		protected override CheckBox CreateView()
		{
			var checkBox = new MauiCheckBox()
			{
				//Style = (System.Windows.Style)System.Windows.Application.Current.MainWindow.FindResource("FormsCheckBoxStyle")
			};
			checkBox.Checked += OnChecked;
			checkBox.Unchecked += OnChecked;
			return checkBox;
		}

		protected override void DisposeView(CheckBox nativeView)
		{
			nativeView.Checked -= OnChecked;
			nativeView.Unchecked -= OnChecked;
			
			base.DisposeView(nativeView);
		}

		public virtual void UpdateColor()
		{
			var color = VirtualView.Color;

			var control = TypedNativeView as MauiCheckBox;

			if (control == null)
				return;

			if (color.IsDefault)
				control.TintBrush = _tintDefaultBrush;
			else
				control.TintBrush = color.ToBrush();

		}

		void UpdateIsChecked()
		{
			TypedNativeView.IsChecked = VirtualView.IsChecked;
		}

		void OnChecked(object sender, System.Windows.RoutedEventArgs e)
		{
			VirtualView.IsChecked = TypedNativeView.IsChecked.HasValue ? TypedNativeView.IsChecked.Value : false;
		}
	}

}
