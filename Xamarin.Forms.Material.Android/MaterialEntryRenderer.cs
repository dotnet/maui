#if __ANDROID_28__
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Material.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Entry), typeof(MaterialEntryRenderer), new[] { typeof(VisualMarker.MaterialVisual) })]
namespace Xamarin.Forms.Material.Android
{
	public sealed class MaterialEntryRenderer : EntryRendererBase<MaterialFormsTextInputLayout>
	{
		MaterialFormsEditText _textInputEditText;
		MaterialFormsTextInputLayout _textInputLayout;

		public MaterialEntryRenderer(Context context) :
			base(MaterialContextThemeWrapper.Create(context))
		{
		}

		protected override MaterialFormsTextInputLayout CreateNativeControl()
		{
			LayoutInflater inflater = LayoutInflater.FromContext(Context);
			var view = inflater.Inflate(Resource.Layout.TextInputLayoutFilledBox, null);
			_textInputLayout = (MaterialFormsTextInputLayout)view;
			_textInputEditText = _textInputLayout.FindViewById<MaterialFormsEditText>(Resource.Id.materialformsedittext);

			return _textInputLayout;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
		{
			base.OnElementChanged(e);
			UpdateBackgroundColor();
		}

		protected override void UpdateColor() => ApplyTheme();

		protected override void UpdateBackgroundColor()
		{
			if (_textInputLayout == null)
				return;

			_textInputLayout.BoxBackgroundColor = MaterialColors.CreateEntryFilledInputBackgroundColor(Element.BackgroundColor, Element.TextColor);
		}

		protected override void UpdatePlaceHolderText() => _textInputLayout.SetHint(Element.Placeholder, Element);
		protected override EditText EditText => _textInputEditText;
		protected override void UpdatePlaceholderColor() => ApplyTheme();
		void ApplyTheme(Color textColor) => _textInputLayout?.ApplyTheme(textColor, Element.PlaceholderColor);
		void ApplyTheme() => ApplyTheme(Element.TextColor);

		protected override void UpdateTextColor(Color color)
		{
			ApplyTheme(color);
		}

		protected override void UpdateFont()
		{
			base.UpdateFont();
			_textInputLayout.Typeface = Element.ToTypeface();
			_textInputEditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

	}
}
#endif