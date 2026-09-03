
using Android.Content;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Material.Android;
using Microsoft.Maui.Controls.Platform.Android;
using AView = Android.Views.View;


namespace Microsoft.Maui.Controls.Compatibility.Material.Android
{
	public class MaterialTimePickerRenderer : TimePickerRendererBase<MaterialPickerTextInputLayout>, ITabStop
	{
		MaterialPickerTextInputLayout _textInputLayout;
		MaterialPickerEditText _textInputEditText;

		public MaterialTimePickerRenderer(Context context) : base(MaterialContextThemeWrapper.Create(context))
		{
		}

		protected override EditText EditText => _textInputEditText;
		protected override AView ControlUsedForAutomation => EditText;

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
			UpdateBackgroundColor();
		}

		protected override void UpdateBackgroundColor() =>
			_textInputLayout?.ApplyBackgroundColor(Element.BackgroundColor, Element.TextColor);

		protected override void UpdateTextColor() => ApplyTheme();

		void ApplyTheme()
		{
			_textInputLayout?.ApplyTheme(Element.TextColor, Color.Default);
		}

		AView ITabStop.TabStop => EditText;
	}
}