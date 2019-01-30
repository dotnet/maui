#if __ANDROID_28__
using System.Threading.Tasks;
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

		protected internal override void UpdateColor() => ApplyTheme();

		protected override void UpdateBackgroundColor()
		{
			if (_textInputLayout == null)
				return;

			_textInputLayout.BoxBackgroundColor = MaterialColors.CreateEntryFilledInputBackgroundColor(Element.BackgroundColor, Element.TextColor);
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
			var textColor = MaterialColors.GetEntryTextColor(Element.TextColor);
			UpdateTextColor(Color.FromUint((uint)textColor.ToArgb()));

			var placeHolderColors = MaterialColors.GetPlaceHolderColor(Element.PlaceholderColor, Element.TextColor);
			var underlineColors = MaterialColors.GetUnderlineColor(Element.TextColor);

			var colors = MaterialColors.CreateEntryUnderlineColors(underlineColors.FocusedColor, underlineColors.UnFocusedColor);

			ViewCompat.SetBackgroundTintList(_textInputEditText, colors);

						
			if (HasFocus || !string.IsNullOrWhiteSpace(_textInputEditText.Text))
				_textInputLayout.DefaultHintTextColor = MaterialColors.CreateEntryFilledPlaceholderColors(placeHolderColors.FloatingColor, placeHolderColors.FloatingColor);
			else
				_textInputLayout.DefaultHintTextColor = MaterialColors.CreateEntryFilledPlaceholderColors(placeHolderColors.InlineColor, placeHolderColors.FloatingColor);
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