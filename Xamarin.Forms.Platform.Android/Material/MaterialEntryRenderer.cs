#if __ANDROID_28__
using Android.Content;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.Material;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Entry), typeof(MaterialEntryRenderer), new[] { typeof(VisualRendererMarker.Material) })]
namespace Xamarin.Forms.Platform.Android.Material
{
	public sealed class MaterialEntryRenderer : EntryRendererBase<MaterialFormsTextInputLayout>
	{
		bool _disposed;
		MaterialFormsEditText _textInputEditText;
		MaterialFormsTextInputLayout _textInputLayout;

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

		protected override void UpdateTextColor() => ApplyTheme();

		protected override void UpdateBackgroundColor()
		{
			if (_textInputLayout == null)
				return;

			_textInputLayout.BoxBackgroundColor = MaterialColors.CreateEntryFilledInputBackgroundColor(Element.BackgroundColor, Element.TextColor);
		}

		protected internal override void UpdatePlaceHolderText()
		{
			_textInputLayout.SetHint(Element.Placeholder, Element);
			Element.InvalidateMeasureNonVirtual(Internals.InvalidationTrigger.VerticalOptionsChanged);
		}

		
		protected override void UpdatePlaceholderColor() => ApplyTheme();
		void ApplyTheme() => _textInputLayout?.ApplyTheme(Element.TextColor, Element.PlaceholderColor);

		protected internal override void UpdateFont()
		{
			base.UpdateFont();
			_textInputLayout.Typeface = Element.ToTypeface();
			_textInputEditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}
	}
}
#endif