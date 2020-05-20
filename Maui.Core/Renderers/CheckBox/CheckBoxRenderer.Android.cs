#if __IOS__
using NativeView = UIKit.UIButton;
#elif __MACOS__
using NativeView = AppKit.NSButton;
#elif MONOANDROID
using NativeView = AndroidX.AppCompat.Widget.AppCompatCheckBox;
#else
using NativeView = System.Object;
#endif

namespace System.Maui.Platform {
	public partial class CheckBoxRenderer
	{
		protected override NativeView CreateView()
		{
			var checkBox = new NativeView(Context);
			checkBox.CheckedChange += OnCheckChanged;
			return checkBox;
		}

		void OnCheckChanged(object sender, Android.Widget.CompoundButton.CheckedChangeEventArgs e) =>
			VirtualView.IsChecked = e.IsChecked;
	}
}
