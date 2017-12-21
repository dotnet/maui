using System;
using AppKit;
using System.ComponentModel;
using Foundation;

namespace Xamarin.Forms.Platform.MacOS
{
	public class PickerRenderer : ViewRenderer<Picker, NSComboBox>
	{
		bool _disposed;
		NSColor _defaultTextColor;
		NSColor _defaultBackgroundColor;

		IElementController ElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
					SetNativeControl(new NSComboBox { Editable = false });

				_defaultTextColor = Control.TextColor;
				_defaultBackgroundColor = Control.BackgroundColor;

				Control.UsesDataSource = true;
				Control.DataSource = new ComboDataSource(this);

				Control.SelectionChanged += ComboBoxSelectionChanged;

				UpdatePicker();
				UpdateFont();
				UpdateTextColor();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Picker.TitleProperty.PropertyName)
				UpdatePicker();
			if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
				UpdatePicker();
			if (e.PropertyName == Picker.TextColorProperty.PropertyName ||
			    e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();
			if (e.PropertyName == Picker.FontSizeProperty.PropertyName ||
				e.PropertyName == Picker.FontFamilyProperty.PropertyName ||
				e.PropertyName == Picker.FontAttributesProperty.PropertyName)
				UpdateFont();
				
		}

		protected override void SetBackgroundColor(Color color)
		{
			base.SetBackgroundColor(color);

			if (Control == null)
				return;

			Control.BackgroundColor = color == Color.Default ? _defaultBackgroundColor : color.ToNSColor();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_disposed)
				{
					_disposed = true;
					if (Element != null)
					{
						//TODO: Implement ObservableList picker source change 
						//((ObservableList<string>)Element.Items).CollectionChanged -= RowsCollectionChanged;
					}

					if (Control != null)
						Control.SelectionChanged -= ComboBoxSelectionChanged;
				}
			}
			base.Dispose(disposing);
		}

		void ComboBoxSelectionChanged(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(Picker.SelectedIndexProperty, (int)Control.SelectedIndex);
		}

		void OnEnded(object sender, EventArgs eventArgs)
		{
			ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController?.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void RowsCollectionChanged(object sender, EventArgs e)
		{
			UpdatePicker();
		}

		void UpdateFont()
		{
			if (Control == null || Element == null)
				return;

			Control.Font = Element.ToNSFont();
		}

		void UpdatePicker()
		{
			if (Control == null || Element == null)
				return;

			var selectedIndex = Element.SelectedIndex;
			var items = Element.Items;
			Control.PlaceholderString = Element.Title ?? string.Empty;
			Control.ReloadData();
			if (items == null || items.Count == 0 || selectedIndex < 0)
				return;

			Control.SelectItem(selectedIndex);
		}

		void UpdateTextColor()
		{
			if (Control == null || Element == null)
				return;

			var textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToNSColor();
		}

		class ComboDataSource : NSComboBoxDataSource
		{
			readonly PickerRenderer _renderer;

			public ComboDataSource(PickerRenderer model)
			{
				_renderer = model;
			}

			public override nint ItemCount(NSComboBox comboBox)
			{
				return _renderer.Element.Items?.Count ?? 0;
			}

			public override NSObject ObjectValueForItem(NSComboBox comboBox, nint index)
			{
				return new NSString(_renderer.Element.Items[(int)index]);
			}

			public override nint IndexOfItem(NSComboBox comboBox, string value)
			{
				var index = _renderer.Element.Items?.IndexOf(value) ?? -1;
				return index;
			}
		}
	}
}