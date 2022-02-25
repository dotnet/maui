using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Handlers
{
	public partial class RadioButtonHandler : ViewHandler<IRadioButton, View>
	{
		RadioButtonFocusChangeListener FocusChangeListener { get; } = new RadioButtonFocusChangeListener();

		static AppCompatRadioButton? GetPlatformRadioButton(IRadioButtonHandler handler) => handler.PlatformView as AppCompatRadioButton;

		protected override AppCompatRadioButton CreatePlatformView()
		{
			return new AppCompatRadioButton(Context)
			{
				SoundEffectsEnabled = false
			};
		}

		protected override void ConnectHandler(View platformView)
		{
			FocusChangeListener.Handler = this;
			platformView.OnFocusChangeListener = FocusChangeListener;

			AppCompatRadioButton? platformRadioButton = GetPlatformRadioButton(this);
			if (platformRadioButton != null)
				platformRadioButton.CheckedChange += OnCheckChanged;
		}

		protected override void DisconnectHandler(View platformView)
		{
			FocusChangeListener.Handler = null;
			platformView.OnFocusChangeListener = null;

			AppCompatRadioButton? platformRadioButton = GetPlatformRadioButton(this);
			if (platformRadioButton != null)
				platformRadioButton.CheckedChange -= OnCheckChanged;
		}

		public static void MapBackground(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			GetPlatformRadioButton(handler)?.UpdateBackground(radioButton);
		}

		public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			GetPlatformRadioButton(handler)?.UpdateIsChecked(radioButton);
		}

		public static void MapContent(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			GetPlatformRadioButton(handler)?.UpdateContent(radioButton);
		}

		public static void MapTextColor(IRadioButtonHandler handler, ITextStyle textStyle)
		{
			GetPlatformRadioButton(handler)?.UpdateTextColor(textStyle);
		}

		public static void MapCharacterSpacing(IRadioButtonHandler handler, ITextStyle textStyle)
		{
			GetPlatformRadioButton(handler)?.UpdateCharacterSpacing(textStyle);
		}

		public static void MapFont(IRadioButtonHandler handler, ITextStyle textStyle)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			GetPlatformRadioButton(handler)?.UpdateFont(textStyle, fontManager);
		}

		public static void MapStrokeColor(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			GetPlatformRadioButton(handler)?.UpdateStrokeColor(radioButton);
		}

		public static void MapStrokeThickness(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			GetPlatformRadioButton(handler)?.UpdateStrokeThickness(radioButton);
		}

		public static void MapCornerRadius(IRadioButtonHandler handler, IRadioButton radioButton)
		{
			GetPlatformRadioButton(handler)?.UpdateCornerRadius(radioButton);
		}

		void OnFocusChanged(bool isFocused)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = isFocused;
		}

		void OnCheckChanged(object? sender, CompoundButton.CheckedChangeEventArgs e)
		{
			if (VirtualView == null)
				return;

			VirtualView.IsChecked = e.IsChecked;
		}

		public class RadioButtonFocusChangeListener : Java.Lang.Object, View.IOnFocusChangeListener
		{
			public RadioButtonHandler? Handler { get; set; }

			public RadioButtonFocusChangeListener()
			{
			}

			public void OnFocusChange(View? v, bool hasFocus)
			{
				if (Handler == null)
					return;

				Handler.OnFocusChanged(hasFocus);
			}
		}
	}
}