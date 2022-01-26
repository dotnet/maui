using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, View>
	{
		AppCompatRadioButton? PlatformRadioButton => (NativeView as AppCompatRadioButton);

		protected override AppCompatRadioButton CreateNativeView()
		{
			return new AppCompatRadioButton(Context)
			{
				SoundEffectsEnabled = false
			};
		}

		protected override void ConnectHandler(View nativeView)
		{
			if (PlatformRadioButton != null)
				PlatformRadioButton.CheckedChange += OnCheckChanged;
		}

		protected override void DisconnectHandler(View nativeView)
		{
			if (PlatformRadioButton != null)
				PlatformRadioButton.CheckedChange -= OnCheckChanged;
		}

		public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.PlatformRadioButton?.UpdateIsChecked(radioButton);
		}

		public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton)
		{
			handler.PlatformRadioButton?.UpdateContent(radioButton);
		}

		public static void MapTextColor(RadioButtonHandler handler, ITextStyle textStyle)
		{
			handler.PlatformRadioButton?.UpdateTextColor(textStyle);
		}

		public static void MapCharacterSpacing(RadioButtonHandler handler, ITextStyle textStyle)
		{
			handler.PlatformRadioButton?.UpdateCharacterSpacing(textStyle);
		}

		public static void MapFont(RadioButtonHandler handler, ITextStyle textStyle)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformRadioButton?.UpdateFont(textStyle, fontManager);
		}

		void OnCheckChanged(object? sender, CompoundButton.CheckedChangeEventArgs e)
		{
			if (VirtualView == null)
				return;

			VirtualView.IsChecked = e.IsChecked;
		}
	}
}