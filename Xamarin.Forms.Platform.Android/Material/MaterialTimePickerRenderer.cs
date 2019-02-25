#if __ANDROID_28__
using System.ComponentModel;
using Android.Content;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.Material;

[assembly: ExportRenderer(typeof(Xamarin.Forms.TimePicker), typeof(MaterialTimePickerRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.Android.Material
{
	public class MaterialTimePickerRenderer : TimePickerRendererBase<MaterialPickerTextInputLayout>
	{
		MaterialPickerTextInputLayout _textInputLayout;
		MaterialPickerEditText _textInputEditText;

		public MaterialTimePickerRenderer(Context context) : base(MaterialContextThemeWrapper.Create(context))
		{
		}

		protected override EditText EditText => _textInputEditText;

		protected override MaterialPickerTextInputLayout CreateNativeControl()
		{
			LayoutInflater inflater = LayoutInflater.FromContext(Context);
			var view = inflater.Inflate(Resource.Layout.MaterialPickerTextInput, null);
			_textInputLayout = (MaterialPickerTextInputLayout)view;
			_textInputEditText = _textInputLayout.FindViewById<MaterialPickerEditText>(Resource.Id.materialformsedittext);
			
			return _textInputLayout;
		}


		protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
		{
			base.OnElementChanged(e);
			_textInputLayout.SetHint(string.Empty, Element);
		}

		protected override void UpdateBackgroundColor()
		{
			if (_textInputLayout == null)
				return;

			_textInputLayout.BoxBackgroundColor = MaterialColors.CreateEntryFilledInputBackgroundColor(Element.BackgroundColor, Element.TextColor);
		}

		protected override void UpdateTextColor() => ApplyTheme();

		void ApplyTheme()
		{
			_textInputLayout?.ApplyTheme(Element.TextColor, Color.Default);
		}

	}
}
#endif