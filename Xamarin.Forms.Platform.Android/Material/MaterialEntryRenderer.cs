#if __ANDROID_28__
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.Material;
using AColor = Android.Graphics.Color;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Entry), typeof(MaterialEntryRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.Android.Material
{
	public sealed class MaterialEntryRenderer : EntryRendererBase<MaterialFormsTextInputLayout>
	{
		// values based on
		// copying to match iOS
		// TODO generalize into xplat classes
		// https://github.com/material-components/material-components-ios/blob/develop/components/TextFields/src/ColorThemer/MDCFilledTextFieldColorThemer.m		
		const float kFilledTextFieldActiveAlpha = 0.87f;
		const float kFilledTextFieldOnSurfaceAlpha = 0.6f;
		const float kFilledTextFieldDisabledAlpha = 0.38f;
		const float kFilledTextFieldSurfaceOverlayAlpha = 0.04f;
		const float kFilledTextFieldIndicatorLineAlpha = 0.42f;
		const float kFilledTextFieldIconAlpha = 0.54f;

		// the idea of this value is that I want Active to be the exact color the user specified
		// and then all the other colors decrease according to the Material theme setup
		static float kFilledPlaceHolderOffset = 1f - kFilledTextFieldActiveAlpha;


		AColor _previousTextColor = AColor.Transparent;

		bool _disposed;
		private MaterialFormsEditText _textInputEditText;
		private MaterialFormsTextInputLayout _textInputLayout;

		public MaterialEntryRenderer(Context context) :
			base(MaterialContextThemeWrapper.Create(context))
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

		IElementController ElementController => Element as IElementController;

		protected override EditText EditText => _textInputEditText;

		protected override MaterialFormsTextInputLayout CreateNativeControl()
		{
			LayoutInflater inflater = LayoutInflater.FromContext(Context);
			var view = inflater.Inflate(Resource.Layout.TextInputLayoutFilledBox, null);
			_textInputLayout = (MaterialFormsTextInputLayout)view;
			_textInputEditText = _textInputLayout.FindViewById<MaterialFormsEditText>(Resource.Id.materialformsedittext);
			_textInputEditText.FocusChange += TextInputEditTextFocusChange;
			_textInputLayout.Hint = Element.Placeholder;

			return _textInputLayout;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);
			var oldElement = e.OldElement;

			if (oldElement != null)
			{
				oldElement.PropertyChanged -= OnElementPropertyChanged;
				oldElement.FocusChangeRequested -= OnFocusChangeRequested;
			}

			if (e.NewElement != null)
				Element.FocusChangeRequested += OnFocusChangeRequested;

			UpdateBackgroundColor();
		}


		protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			e.Result = true;

			if (e.Focus)
			{
				// use post being BeginInvokeOnMainThread will not delay on android
				Looper looper = Context.MainLooper;
				var handler = new Handler(looper);
				handler.Post(() =>
				{
					_textInputEditText.RequestFocus();
				});
			}
			else
			{
				_textInputEditText.ClearFocus();
			}

			if (e.Focus)
				this.ShowKeyboard();
			else
				this.HideKeyboard();
		}


		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing && Element != null)
				Element.FocusChangeRequested -= OnFocusChangeRequested;

			base.Dispose(disposing);
		}


		void TextInputEditTextFocusChange(object sender, FocusChangeEventArgs e)
		{
			// TODO figure out better way to do this
			// this is a hack that changes the active underline color from the accent color to whatever the user 
			// specified
			Device.BeginInvokeOnMainThread(() => UpdatePlaceholderColor());
		}

		AColor TextColor => Element.TextColor != Color.Default ? Element.TextColor.ToAndroid() : MaterialColors.Light.PrimaryColor;

		protected internal override void UpdateColor() => ApplyTheme();

		protected override void UpdateBackgroundColor()
		{
			if (_textInputLayout == null)
				return;

			if (Element.BackgroundColor == Color.Default)
			{
				if (Element.TextColor != Color.Default)
					_textInputLayout.BoxBackgroundColor = MaterialColors.CreateEntryFilledInputBackgroundColor(TextColor);
				else
					_textInputLayout.BoxBackgroundColor = MaterialColors.CreateEntryFilledInputBackgroundColor(MaterialColors.Light.PrimaryColorVariant);
			}
			else
			{
				_textInputLayout.BoxBackgroundColor = Element.BackgroundColor.ToAndroid();
			}
		}

		protected internal override void UpdatePlaceHolderText()
		{
			_textInputLayout.Hint = Element.Placeholder;
		}

		protected internal override void UpdatePlaceholderColor() => ApplyTheme();

		void ApplyTheme()
		{
			if (_textInputLayout == null)
				return;

			// set text color
			var textColor = TextColor;
			UpdateTextColor(Color.FromUint((uint)textColor.ToArgb()));
			var colors = MaterialColors.CreateEntryUnderlineColors(textColor, textColor.WithAlpha(kFilledTextFieldOnSurfaceAlpha));

			// Ensure that we SetBackgroundTintList when focused to override the themes accent color which gets
			// applied to the underline
			if (_previousTextColor != textColor)
			{
				if(HasFocus)
					_previousTextColor = textColor;				

				ViewCompat.SetBackgroundTintList(_textInputEditText, colors);
			}

			// set placeholder color
			AColor placeholderColor;
			if (Element.PlaceholderColor == Color.Default)
				if (Element.TextColor == Color.Default)
					placeholderColor = MaterialColors.Light.OnSurfaceColor;
				else
					placeholderColor = textColor;
			else
				placeholderColor = Element.PlaceholderColor.ToAndroid();

			if (!HasFocus)
				placeholderColor = placeholderColor.WithAlpha(kFilledTextFieldOnSurfaceAlpha + kFilledPlaceHolderOffset);

			_textInputLayout.DefaultHintTextColor = MaterialColors.CreateEntryFilledPlaceholderColors(placeholderColor);
		}

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			_textInputLayout.Typeface = Element.ToTypeface();
			_textInputEditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}
	}
}
#endif