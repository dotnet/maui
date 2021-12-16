using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, AppCompatRadioButton>
	{
		protected override AppCompatRadioButton CreateNativeView()
		{
			return new AppCompatRadioButton(Context)
			{
				SoundEffectsEnabled = false
			};
		}

		protected override void ConnectHandler(AppCompatRadioButton nativeView)
		{
			nativeView.CheckedChange += OnCheckChanged;
		}

		protected override void DisconnectHandler(AppCompatRadioButton nativeView)
		{
			nativeView.CheckedChange -= OnCheckChanged;
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.NativeView?.UpdateIsChecked(radioButton);
		}

		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.NativeView?.UpdateContent(radioButton);
		}

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle)
		{
			handler.NativeView?.UpdateTextColor(textStyle);
		}

		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle)
		{
			handler.NativeView?.UpdateCharacterSpacing(textStyle);
		}

		public static void MapFont(RadioButtonHandler handler, ITextStyle textStyle)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(textStyle, fontManager);
		}

		void OnCheckChanged(object? sender, CompoundButton.CheckedChangeEventArgs e)
		{
			if (VirtualView == null)
				return;

			VirtualView.IsChecked = e.IsChecked;
		}
	}
}