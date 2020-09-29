using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using AColor = Android.Graphics.Color;
using Orientation = Android.Widget.Orientation;

namespace Xamarin.Forms.Platform.Android
{
	public class PickerRenderer : ViewRenderer<Picker, EditText>, IPickerRenderer
	{
		AlertDialog _dialog;
		bool _isDisposed;
		TextColorSwitcher _textColorSwitcher;
		int _originalHintTextColor;
		EntryAccessibilityDelegate _pickerAccessibilityDelegate;

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

		IElementController ElementController => Element as IElementController;

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_isDisposed)
			{
				_isDisposed = true;
				((INotifyCollectionChanged)Element.Items).CollectionChanged -= RowsCollectionChanged;

				_pickerAccessibilityDelegate?.Dispose();
				_pickerAccessibilityDelegate = null;
			}

			base.Dispose(disposing);
		}

		protected override EditText CreateNativeControl()
		{
			return new PickerEditText(Context);
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

					textField.SetAccessibilityDelegate(_pickerAccessibilityDelegate = new EntryAccessibilityDelegate(Element));

					var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
					_textColorSwitcher = new TextColorSwitcher(textField.TextColors, useLegacyColorManagement);

					SetNativeControl(textField);

					_originalHintTextColor = Control.CurrentHintTextColor;
				}

				UpdateFont();
				UpdatePicker();
				UpdateTextColor();
				UpdateCharacterSpacing();
				UpdateGravity();
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
			else if (e.PropertyName == Picker.CharacterSpacingProperty.PropertyName)
				UpdateCharacterSpacing();
			else if (e.PropertyName == Picker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Picker.FontAttributesProperty.PropertyName || e.PropertyName == Picker.FontFamilyProperty.PropertyName || e.PropertyName == Picker.FontSizeProperty.PropertyName)
				UpdateFont();
			else if (e.PropertyName == Picker.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == Picker.VerticalTextAlignmentProperty.PropertyName)
				UpdateGravity();
		}

		protected override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
			{
				if (Clickable)
					CallOnClick();
				else
					((IPickerRenderer)this)?.OnClick();
			}
			else if (_dialog != null)
			{
				_dialog.Hide();
				ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				_dialog = null;
			}
		}

		void IPickerRenderer.OnClick()
		{
			Picker model = Element;

			if (_dialog != null)
				return;

			var picker = new NumberPicker(Context);
			if (model.Items != null && model.Items.Any())
			{
				picker.MaxValue = model.Items.Count - 1;
				picker.MinValue = 0;
				picker.SetDisplayedValues(model.Items.ToArray());
				picker.WrapSelectorWheel = false;
				picker.DescendantFocusability = DescendantFocusability.BlockDescendants;
				picker.Value = model.SelectedIndex;
			}

			var layout = new LinearLayout(Context) { Orientation = Orientation.Vertical };
			layout.AddView(picker);

			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);

			var builder = new AlertDialog.Builder(Context);
			builder.SetView(layout);

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

			builder.SetNegativeButton(global::Android.Resource.String.Cancel, (s, a) =>
			{
				ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				_dialog = null;
			});
			builder.SetPositiveButton(global::Android.Resource.String.Ok, (s, a) =>
			{
				ElementController.SetValueFromRenderer(Picker.SelectedIndexProperty, picker.Value);
				// It is possible for the Content of the Page to be changed on SelectedIndexChanged. 
				// In this case, the Element & Control will no longer exist.
				if (Element != null)
				{
					if (model.Items.Count > 0 && Element.SelectedIndex >= 0)
						Control.Text = model.Items[Element.SelectedIndex];
					ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				}
				_dialog = null;
			});

			_dialog = builder.Create();
			_dialog.DismissEvent += (sender, args) =>
			{
				ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				_dialog?.Dispose();
				_dialog = null;
			};
			_dialog.Show();
		}

		void RowsCollectionChanged(object sender, EventArgs e)
		{
			UpdatePicker();
		}

		void UpdateCharacterSpacing()
		{
			if (Forms.IsLollipopOrNewer)
			{
				Control.LetterSpacing = Element.CharacterSpacing.ToEm();
			}
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

			string oldText = Control.Text;

			if (Element.SelectedIndex == -1 || Element.Items == null || Element.SelectedIndex >= Element.Items.Count)
				Control.Text = null;
			else
				Control.Text = Element.Items[Element.SelectedIndex];

			if (oldText != Control.Text)
				((IVisualElementController)Element).NativeSizeChanged();

			_pickerAccessibilityDelegate.ValueText = Control.Text;
		}

		void UpdateTextColor()
		{
			_textColorSwitcher?.UpdateTextColor(Control, Element.TextColor);
		}

		void UpdateGravity()
		{
			Control.Gravity = Element.HorizontalTextAlignment.ToHorizontalGravityFlags() | Element.VerticalTextAlignment.ToVerticalGravityFlags();
		}
	}
}