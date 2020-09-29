
using Android.Content;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Material.Android;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;


namespace Xamarin.Forms.Material.Android
{
	public class MaterialPickerRenderer : Platform.Android.AppCompat.PickerRendererBase<MaterialPickerTextInputLayout>, ITabStop
	{
		MaterialPickerTextInputLayout _textInputLayout;
		MaterialPickerEditText _textInputEditText;

		public MaterialPickerRenderer(Context context) : base(MaterialContextThemeWrapper.Create(context))
		{
		}

		protected override EditText EditText => _textInputEditText;
		protected override AView ControlUsedForAutomation => EditText;

		protected override MaterialPickerTextInputLayout CreateNativeControl()
		{
			var inflater = LayoutInflater.FromContext(Context);
			var view = inflater.Inflate(Resource.Layout.MaterialPickerTextInput, null);
			_textInputLayout = (MaterialPickerTextInputLayout)view;
			_textInputEditText = _textInputLayout.FindViewById<MaterialPickerEditText>(Resource.Id.materialformsedittext);

			return _textInputLayout;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			base.OnElementChanged(e);
			UpdateBackgroundColor();
		}

		protected override void UpdateBackgroundColor()
		{
			if (_textInputLayout == null)
				return;

			_textInputLayout.BoxBackgroundColor = MaterialColors.CreateEntryFilledInputBackgroundColor(Element.BackgroundColor, Element.TextColor);
		}

		protected override void UpdatePlaceHolderText()
		{
			_textInputLayout.SetHint(Element.Title, Element);
		}

		protected override void UpdateTitleColor() => ApplyTheme();
		protected override void UpdateTextColor() => ApplyTheme();
		protected virtual void ApplyTheme() => _textInputLayout?.ApplyTheme(Element.TextColor, Element.TitleColor);

		AView ITabStop.TabStop => EditText;

		protected override void UpdateGravity()
		{
			_textInputEditText.Gravity = Element.HorizontalTextAlignment.ToHorizontalGravityFlags() | Element.VerticalTextAlignment.ToVerticalGravityFlags();
		}
	}
}