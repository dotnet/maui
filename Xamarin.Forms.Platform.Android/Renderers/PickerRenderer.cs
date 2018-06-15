using Android.App;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ADatePicker = Android.Widget.DatePicker;
using ATimePicker = Android.Widget.TimePicker;
using Object = Java.Lang.Object;
using Orientation = Android.Widget.Orientation;
using Android.Content;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Platform.Android
{
	public class PickerRenderer : ViewRenderer<Picker, EditText>
	{
		AlertDialog _dialog;
		bool _isDisposed;
		TextColorSwitcher _textColorSwitcher;

		public PickerRenderer(Context context) : base(context)
		{
			AutoPackage = false;
		}

		[Obsolete("This constructor is obsolete as of version 2.5. Please use PickerRenderer(Context) instead.")]
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
			}

			base.Dispose(disposing);
		}

		protected override EditText CreateNativeControl()
		{
			return new EditText(Context) { Focusable = false, Clickable = true, Tag = this };
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
					textField.SetOnClickListener(PickerListener.Instance);

					var useLegacyColorManagement = e.NewElement.UseLegacyColorManagement();
					_textColorSwitcher = new TextColorSwitcher(textField.TextColors, useLegacyColorManagement);

					SetNativeControl(textField);
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

			if (e.PropertyName == Picker.TitleProperty.PropertyName)
				UpdatePicker();
			else if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
				UpdatePicker();
			else if (e.PropertyName == Picker.TextColorProperty.PropertyName)
				UpdateTextColor();
			else if (e.PropertyName == Picker.FontAttributesProperty.PropertyName || e.PropertyName == Picker.FontFamilyProperty.PropertyName || e.PropertyName == Picker.FontSizeProperty.PropertyName)
				UpdateFont();
		}

		internal override void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			base.OnFocusChangeRequested(sender, e);

			if (e.Focus)
				OnClick();
			else if (_dialog != null)
			{
				_dialog.Hide();
				ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				Control.ClearFocus();
				_dialog = null;
			}
		}

		void OnClick()
		{
			Picker model = Element;

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
			builder.SetTitle(model.Title ?? "");
			builder.SetNegativeButton(global::Android.Resource.String.Cancel, (s, a) =>
			{
				ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
				// It is possible for the Content of the Page to be changed when Focus is changed.
				// In this case, we'll lose our Control.
				Control?.ClearFocus();
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
					// It is also possible for the Content of the Page to be changed when Focus is changed.
					// In this case, we'll lose our Control.
					Control?.ClearFocus();
				}
				_dialog = null;
			});

			_dialog = builder.Create();
			_dialog.DismissEvent += (sender, args) =>
			{
				ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
			};
			_dialog.Show();
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

			string oldText = Control.Text;

			if (Element.SelectedIndex == -1 || Element.Items == null || Element.SelectedIndex >= Element.Items.Count)
				Control.Text = null;
			else
				Control.Text = Element.Items[Element.SelectedIndex];

			if (oldText != Control.Text)
				((IVisualElementController)Element).NativeSizeChanged();
		}

		void UpdateTextColor()
		{
			_textColorSwitcher?.UpdateTextColor(Control, Element.TextColor);
		}

		class PickerListener : Object, IOnClickListener
		{
			public static readonly PickerListener Instance = new PickerListener();

			public void OnClick(global::Android.Views.View v)
			{
				var renderer = v.Tag as PickerRenderer;
				if (renderer == null)
					return;

				renderer.OnClick();
			}
		}
	}
}