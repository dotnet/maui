#if __ANDROID_28__
using System.ComponentModel;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.Material;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Picker), typeof(MaterialPickerRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialPickerRenderer : AppCompat.PickerRendererBase<MaterialPickerTextInputLayout>
	{
		MaterialPickerTextInputLayout _textInputLayout;
		MaterialPickerEditText _textInputEditText;

		public MaterialPickerRenderer(Context context) : base(MaterialContextThemeWrapper.Create(context))
		{
		}


		protected override EditText EditText => _textInputEditText;

		protected override MaterialPickerTextInputLayout CreateNativeControl()
		{
			var inflater = LayoutInflater.FromContext(Context);
			var view = inflater.Inflate(Resource.Layout.MaterialPickerTextInput, null);
			_textInputLayout = (MaterialPickerTextInputLayout)view;
			_textInputEditText = _textInputLayout.FindViewById<MaterialPickerEditText>(Resource.Id.materialformsedittext);
			
			return _textInputLayout;
		}

		protected override void UpdateBackgroundColor()
		{
			if (_textInputLayout == null)
				return;

			_textInputLayout.BoxBackgroundColor = MaterialColors.CreateEntryFilledInputBackgroundColor(Element.BackgroundColor, Element.TextColor);
		}

		protected internal override void UpdatePlaceHolderText()
		{
			_textInputLayout.SetHint(string.Empty, Element);
		}

		protected internal override void UpdateTitleColor() => ApplyTheme();
		protected override void UpdateTextColor() => ApplyTheme();

		void ApplyTheme() => _textInputLayout?.ApplyTheme(Element.TextColor, Color.Default);
	}
}
#endif