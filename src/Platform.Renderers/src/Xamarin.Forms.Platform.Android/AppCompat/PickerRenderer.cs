using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Widget;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
	public abstract class PickerRendererBase<TControl> : ViewRenderer<Picker, TControl>, IPickerRenderer
		where TControl : global::Android.Views.View
	{
		AlertDialog _dialog;
		bool _disposed;
		EntryAccessibilityDelegate _pickerAccessibilityDelegate;

		public PickerRendererBase(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use PickerRenderer(Context) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public PickerRendererBase()
		{
			AutoPackage = false;
		}

		protected abstract EditText EditText { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;

				((INotifyCollectionChanged)Element.Items).CollectionChanged -= RowsCollectionChanged;

				_pickerAccessibilityDelegate?.Dispose();
				_pickerAccessibilityDelegate = null;
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

					SetNativeControl(textField);

					ControlUsedForAutomation.SetAccessibilityDelegate(_pickerAccessibilityDelegate = new EntryAccessibilityDelegate(Element));
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
			if (this.IsDisposed())
			{
				return;
			}

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
					_dialog?.Dispose();
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
			EditText.Typeface = Element.ToTypeface();
			EditText.SetTextSize(ComplexUnitType.Sp, (float)Element.FontSize);
		}

		protected void UpdateCharacterSpacing()
		{
			if (Forms.IsLollipopOrNewer)
			{
				EditText.LetterSpacing = Element.CharacterSpacing.ToEm();
			}
		}

		void UpdatePicker()
		{
			UpdatePlaceHolderText();
			UpdateTitleColor();

			if (Element.SelectedIndex == -1 || Element.Items == null || Element.SelectedIndex >= Element.Items.Count)
				EditText.Text = null;
			else
				EditText.Text = Element.Items[Element.SelectedIndex];

			_pickerAccessibilityDelegate.ValueText = EditText.Text;
		}

		abstract protected void UpdateTextColor();
		abstract protected void UpdateTitleColor();
		abstract protected void UpdatePlaceHolderText();
		abstract protected void UpdateGravity();
	}

	public class PickerRenderer : PickerRendererBase<EditText>
	{
		TextColorSwitcher _textColorSwitcher;
		TextColorSwitcher _hintColorSwitcher;

		[Obsolete("This constructor is obsolete as of version 2.5. Please use PickerRenderer(Context) instead.")]
		public PickerRenderer()
		{
		}

		public PickerRenderer(Context context) : base(context)
		{
		}

		protected override EditText CreateNativeControl()
		{
			return new PickerEditText(Context);
		}

		protected override EditText EditText => Control;

		protected override void UpdateTitleColor()
		{
			_hintColorSwitcher = _hintColorSwitcher ?? new TextColorSwitcher(EditText.HintTextColors, Element.UseLegacyColorManagement());
			_hintColorSwitcher.UpdateTextColor(EditText, Element.TitleColor, EditText.SetHintTextColor);
		}

		protected override void UpdateTextColor()
		{
			_textColorSwitcher = _textColorSwitcher ?? new TextColorSwitcher(EditText.TextColors, Element.UseLegacyColorManagement());
			_textColorSwitcher.UpdateTextColor(EditText, Element.TextColor);
		}
		protected override void UpdatePlaceHolderText()
		{
			EditText.Hint = Element.Title;
		}

		protected override void UpdateGravity()
		{
			EditText.Gravity = Element.HorizontalTextAlignment.ToHorizontalGravityFlags() | Element.VerticalTextAlignment.ToVerticalGravityFlags();
		}
	}
}