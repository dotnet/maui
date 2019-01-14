using Android.App;
using Android.Util;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.Content;
using Android.Widget;
using AColor = Android.Graphics.Color;
using Android.Text;
using Android.Text.Style;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public class PickerRenderer : ViewRenderer<Picker, EditText>, IPickerRenderer
	{
		AlertDialog _dialog;
		bool _disposed;
		TextColorSwitcher _textColorSwitcher;
		int _originalHintTextColor;

		public PickerRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use PickerRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public PickerRenderer()
		{
			AutoPackage = false;
		}

		protected override EditText CreateNativeControl()
		{
			return new PickerEditText(Context, this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				((INotifyCollectionChanged)Element.Items).CollectionChanged -= RowsCollectionChanged;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (e.OldElement != null)
				((INotifyCollectionChanged)e.OldElement.Items).CollectionChanged -= RowsCollectionChanged;

			if (e.NewElement != null)
			{
				((INotifyCollectionChanged)e.NewElement.Items).CollectionChanged += RowsCollectionChanged;
				if (Control == null)
				{
					var textField = CreateNativeControl();

					var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
					_textColorSwitcher = new TextColorSwitcher(textField.TextColors, useLegacyColorManagement);

					SetNativeControl(textField);

					_originalHintTextColor = Control.CurrentHintTextColor;
				}
				UpdateFont();
				UpdatePicker();
				UpdateTextColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Picker.TitleProperty.PropertyName || e.PropertyName == Picker.TitleColorProperty.PropertyName)
				UpdatePicker();
			else if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
				UpdatePicker();
			else if (e.PropertyName == Picker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Picker.FontAttributesProperty.PropertyName || e.PropertyName == Picker.FontFamilyProperty.PropertyName || e.PropertyName == Picker.FontSizeProperty.PropertyName)
				UpdateFont();
		}

		protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
				CallOnClick();
			else if (_dialog != null)
			{
				_dialog.Hide();
				((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				Control.ClearFocus();
				_dialog = null;
			}
		}

		void IPickerRenderer.OnClick()
		{
			Picker model = Element;
			if (_dialog == null)
			{
				using (var builder = new AlertDialog.Builder(Context))
				{
					if (!Element.IsSet(Picker.TitleColorProperty))
					{
						builder.SetTitle(model.Title ?? "");
					}
					else
					{
						var title = new SpannableString(model.Title ?? "");
						title.SetSpan(new ForegroundColorSpan(model.TitleColor.ToAndroid()), 0, title.Length(), SpanTypes.ExclusiveExclusive);

						builder.SetTitle(title);
					}

					string[] items = model.Items.ToArray();
					builder.SetItems(items, (s, e) => ((IElementController)model).SetValueFromRenderer(Picker.SelectedIndexProperty, e.Which));

					builder.SetNegativeButton(global::Android.Resource.String.Cancel, (o, args) => { });

					((IElementController)Element).SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);

					_dialog = builder.Create();
				}
				_dialog.SetCanceledOnTouchOutside(true);
				_dialog.DismissEvent += (sender, args) =>
				{
					(Element as IElementController)?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
					_dialog.Dispose();
					_dialog = null;
				};

				_dialog.Show();
			}
		}

		void RowsCollectionChanged(object sender, EventArgs e)
		{
			UpdatePicker();
		}

		void UpdateFont()
		{
			Control.Typeface = Element.ToTypeface();
			Control.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		void UpdatePicker()
		{
			Control.Hint = Element.Title;

			if (Element.IsSet(Picker.TitleColorProperty))
				Control.SetHintTextColor(Element.TitleColor.ToAndroid());
			else
				Control.SetHintTextColor(new AColor(_originalHintTextColor));

			if (Element.SelectedIndex == -1 || Element.Items == null || Element.SelectedIndex >= Element.Items.Count)
				Control.Text = null;
			else
				Control.Text = Element.Items[Element.SelectedIndex];
		}

		void UpdateTextColor()
		{
			_textColorSwitcher?.UpdateTextColor(Control, Element.TextColor);
		}
	}
}