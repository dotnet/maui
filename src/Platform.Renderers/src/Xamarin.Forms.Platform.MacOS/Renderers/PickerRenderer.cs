using System;
using AppKit;
using System.ComponentModel;
using Foundation;
using System.Collections.Specialized;
using System.Linq;

namespace Xamarin.Forms.Platform.MacOS
{
	public class PickerRenderer : ViewRenderer<Picker, NSPopUpButton>
	{
		bool _disposed;
		NSColor _defaultBackgroundColor;

		IElementController ElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (e.OldElement != null)
				((INotifyCollectionChanged)e.OldElement.Items).CollectionChanged -= RowsCollectionChanged;

			if (e.NewElement != null)
			{
				if (Control == null)
					SetNativeControl(new NSPopUpButton());

				_defaultBackgroundColor = Control.Cell.BackgroundColor;

				Control.Activated -= ComboBoxSelectionChanged;
				Control.Activated += ComboBoxSelectionChanged;
				UpdatePicker();
				UpdateFont();
				UpdateTextColor();

				((INotifyCollectionChanged)e.NewElement.Items).CollectionChanged -= RowsCollectionChanged;
				((INotifyCollectionChanged)e.NewElement.Items).CollectionChanged += RowsCollectionChanged;
			}

			base.OnElementChanged(e);
		}

		private void UpdateItems()
		{
			if (Control == null || Element == null)
				return;

			Control.RemoveAllItems();
			Control.AddItems(Element.Items.ToArray());
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Picker.TitleProperty.PropertyName)
				UpdatePicker();
			if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
				UpdatePicker();
			if (e.PropertyName == Picker.TextColorProperty.PropertyName ||
				e.PropertyName == VisualElement.IsEnabledProperty.PropertyName ||
				e.PropertyName == Picker.SelectedItemProperty.PropertyName)
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

			Control.Cell.BackgroundColor = color == Color.Default ? _defaultBackgroundColor : color.ToNSColor();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!_disposed)
				{
					_disposed = true;
					if (Element != null)
						((INotifyCollectionChanged)Element.Items).CollectionChanged -= RowsCollectionChanged;

					if (Control != null)
						Control.Activated -= ComboBoxSelectionChanged;
				}
			}
			base.Dispose(disposing);
		}

		void ComboBoxSelectionChanged(object sender, EventArgs e)
		{
			ElementController?.SetValueFromRenderer(Picker.SelectedIndexProperty, (int)Control.IndexOfSelectedItem);
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

			//Control.Menu.Font = Element.ToNSFont();
		}

		void UpdatePicker()
		{
			if (Control == null || Element == null)
				return;

			var selectedIndex = Element.SelectedIndex;
			var items = Element.Items;
			UpdateItems();

			if (items == null || items.Count == 0 || selectedIndex < 0)
				return;

			Control.SelectItem(selectedIndex);
		}

		void UpdateTextColor()
		{
			if (Control == null || Element == null)
				return;

			foreach (NSMenuItem it in Control.Items())
			{
				it.AttributedTitle = new NSAttributedString();
			}

			var color = Element.TextColor;
			if (color != Color.Default && Control.SelectedItem != null)
			{
				NSAttributedString textWithColor = new NSAttributedString(Control.SelectedItem.Title, foregroundColor: color.ToNSColor(), paragraphStyle: new NSMutableParagraphStyle() { Alignment = NSTextAlignment.Left });
				Control.SelectedItem.AttributedTitle = textWithColor;
			}
		}
	}
}