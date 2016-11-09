using System;
using System.Collections.Specialized;
using System.ComponentModel;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Xamarin.Forms.Platform.iOS
{
	public class PickerRenderer : ViewRenderer<Picker, UITextField>
	{
		UIPickerView _picker;
		UIColor _defaultTextColor;

		IElementController ElementController => Element as IElementController;

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (e.OldElement != null)
				((INotifyCollectionChanged)e.OldElement.Items).CollectionChanged -= RowsCollectionChanged;

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var entry = new NoCaretField { BorderStyle = UITextBorderStyle.RoundedRect };

					entry.EditingDidBegin += OnStarted;
					entry.EditingDidEnd += OnEnded;

					_picker = new UIPickerView();

					var width = UIScreen.MainScreen.Bounds.Width;
					var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
					var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
					var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
					{
						var s = (PickerSource)_picker.Model;
						if (s.SelectedIndex == -1 && Element.Items != null && Element.Items.Count > 0)
							UpdatePickerSelectedIndex(0);
						UpdatePickerFromModel(s);
						entry.ResignFirstResponder();
					});

					toolbar.SetItems(new[] { spacer, doneButton }, false);

					entry.InputView = _picker;
					entry.InputAccessoryView = toolbar;

					_defaultTextColor = entry.TextColor;

					SetNativeControl(entry);
				}

				_picker.Model = new PickerSource(this);

				UpdatePicker();
				UpdateTextColor();

				((INotifyCollectionChanged)e.NewElement.Items).CollectionChanged += RowsCollectionChanged;
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
			if (e.PropertyName == Picker.TextColorProperty.PropertyName || e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
				UpdateTextColor();
		}

		void OnEnded(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
		}

		void OnStarted(object sender, EventArgs eventArgs)
		{
			ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
		}

		void RowsCollectionChanged(object sender, EventArgs e)
		{
			UpdatePicker();
		}

		void UpdatePicker()
		{
			var selectedIndex = Element.SelectedIndex;
			var items = Element.Items;
			Control.Placeholder = Element.Title;
			var oldText = Control.Text;
			Control.Text = selectedIndex == -1 || items == null ? "" : items[selectedIndex];
			UpdatePickerNativeSize(oldText);
			_picker.ReloadAllComponents();
			if (items == null || items.Count == 0)
				return;

			UpdatePickerSelectedIndex(selectedIndex);
		}

		void UpdatePickerFromModel(PickerSource s)
		{
			if (Element != null)
			{
				var oldText = Control.Text;
				ElementController.SetValueFromRenderer(Picker.SelectedIndexProperty, s.SelectedIndex);
				Control.Text = s.SelectedItem;
				UpdatePickerNativeSize(oldText);
			}
		}

		void UpdatePickerNativeSize(string oldText)
		{
			if (oldText != Control.Text)
				((IVisualElementController)Element).NativeSizeChanged();
		}

		void UpdatePickerSelectedIndex(int formsIndex)
		{
			var source = (PickerSource)_picker.Model;
			source.SelectedIndex = formsIndex;
			source.SelectedItem = formsIndex >= 0 ? Element.Items[formsIndex] : null;
			_picker.Select(Math.Max(formsIndex, 0), 0, true);
		}

		void UpdateTextColor()
		{
			var textColor = Element.TextColor;

			if (textColor.IsDefault || !Element.IsEnabled)
				Control.TextColor = _defaultTextColor;
			else
				Control.TextColor = textColor.ToUIColor();
		}

		class PickerSource : UIPickerViewModel
		{
			readonly PickerRenderer _renderer;

			public PickerSource(PickerRenderer model)
			{
				_renderer = model;
			}

			public int SelectedIndex { get; internal set; }

			public string SelectedItem { get; internal set; }

			public override nint GetComponentCount(UIPickerView picker)
			{
				return 1;
			}

			public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
			{
				return _renderer.Element.Items != null ? _renderer.Element.Items.Count : 0;
			}

			public override string GetTitle(UIPickerView picker, nint row, nint component)
			{
				return _renderer.Element.Items[(int)row];
			}

			public override void Selected(UIPickerView picker, nint row, nint component)
			{
				if (_renderer.Element.Items.Count == 0)
				{
					SelectedItem = null;
					SelectedIndex = -1;
				}
				else
				{
					SelectedItem = _renderer.Element.Items[(int)row];
					SelectedIndex = (int)row;
				}
				_renderer.UpdatePickerFromModel(this);
			}
		}
	}
}