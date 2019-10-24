using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class PickerRenderer : ViewRenderer<Picker, FrameworkElement>
	{
		bool _isChanging;
		FormsListPicker _listPicker;
		Brush _defaultBrush;

		IElementController ElementController => Element as IElementController;

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			_listPicker = new FormsListPicker();

			UpdateAlignment();
			UpdateIsEnabled();

			base.OnElementChanged(e);

			if (e.OldElement != null)
				((INotifyCollectionChanged)Element.Items).CollectionChanged -= ItemsCollectionChanged;

			((INotifyCollectionChanged)Element.Items).CollectionChanged += ItemsCollectionChanged;

			_listPicker.ItemTemplate = (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["PickerItemTemplate"];
			_listPicker.FullModeItemTemplate = (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["PickerFullItemTemplate"];
			_listPicker.ExpansionMode = ExpansionMode.FullScreenOnly;
			_listPicker.Items.Add(new ItemViewModel(" ") { MaxHeight = 0 });

			_listPicker.ListPickerModeChanged += ListPickerModeChanged;
			_listPicker.Loaded += (sender, args) => {
				// The defaults from the control template won't be available
				// right away; we have to wait until after the template has been applied
				_defaultBrush = _listPicker.Foreground;
				UpdateTextColor();
			};

			var grid = new System.Windows.Controls.Grid { Children = { _listPicker }, MaxWidth = Device.Info.PixelScreenSize.Width };
			SetNativeControl(grid);

			UpdatePicker();
			_listPicker.SelectionChanged += PickerSelectionChanged;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Picker.TitleProperty.PropertyName)
			{
				_listPicker.FullModeHeader = Element.Title;
			}
			else if (e.PropertyName == VisualElement.IsEnabledProperty.PropertyName)
			{
				UpdateIsEnabled();
				UpdateTextColor();
			}
			else if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
			{
				if (Element.SelectedIndex >= 0 && Element.SelectedIndex < Element.Items.Count)
					_listPicker.SelectedIndex = Element.SelectedIndex + 1;
			}
			else if (e.PropertyName == View.HorizontalOptionsProperty.PropertyName)
			{
				UpdateAlignment();
			}
			else if (e.PropertyName == Picker.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
		}

		protected override void OnGotFocus(object sender, RoutedEventArgs args)
		{
			// Do nothing. ListPickerModeChanged is handling the IsFocusProperty setter
			// Required because FrameworkElement.GotFocus and FrameworkElement.LostFocus () are fired by ListPicker.Open ()
		}

		protected override void OnLostFocus(object sender, RoutedEventArgs args)
		{
			// Do nothing. ListPickerModeChanged is handling the IsFocusProperty setter
			// Required because FrameworkElement.GotFocus and FrameworkElement.LostFocus () are fired by ListPicker.Open ()
		}

		protected override void UpdateNativeWidget()
		{
			base.UpdateNativeWidget();
			UpdateIsEnabled();
		}

		internal override void OnModelFocusChangeRequested(object sender, VisualElement.FocusRequestArgs args)
		{
			if (Control == null)
				return;

			if (args.Focus)
				args.Result = OpenPickerPage();
			else
			{
				args.Result = ClosePickerPage();
				UnfocusControl(_listPicker);
			}
		}

		bool ClosePickerPage()
		{
			FieldInfo pickerPageField = typeof(ListPicker).GetField("_listPickerPage", BindingFlags.NonPublic | BindingFlags.Instance);
			var pickerPage = pickerPageField.GetValue(Control) as ListPickerPage;
			typeof(ListPickerPage).InvokeMember("ClosePickerPage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, Type.DefaultBinder, pickerPage, null);

			return true;
		}

		void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateItems();
		}

		void ListPickerModeChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue == null || e.NewValue == null)
				return;

			var oldVal = (ListPickerMode)e.OldValue;
			var newVal = (ListPickerMode)e.NewValue;

			if (oldVal == ListPickerMode.Normal && newVal == ListPickerMode.Full)
			{
				// Picker Page is now showing
				ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, true);
			}
			else if (oldVal == ListPickerMode.Full && newVal == ListPickerMode.Normal)
			{
				// PickerPage is now dismissed
				ElementController.SetValueFromRenderer(VisualElement.IsFocusedPropertyKey, false);
			}
		}

		bool OpenPickerPage()
		{
			bool result = _listPicker.Open();

			if (result)
				return true;

			return false;
		}

		void PickerSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (_isChanging)
				return;

			var picker = (ListPicker)sender;

			// initializing picker
			if (picker.SelectedIndex == -1)
				return;

			int elementSelectedIndex = picker.SelectedIndex - 1;
			ElementController.SetValueFromRenderer(Picker.SelectedIndexProperty, elementSelectedIndex);
		}

		void UpdateAlignment()
		{
			if (Element.HorizontalOptions.Alignment != LayoutAlignment.Fill)
				_listPicker.HorizontalAlignment = HorizontalAlignment.Left;
		}

		void UpdateIsEnabled()
		{
			if (_listPicker != null)
				_listPicker.IsEnabled = Element.IsEnabled;
		}

		void UpdateItems()
		{
			// supress notification of non-user generated events (e.g. adding\syncing list values)
			_isChanging = true;
			FormsListPicker picker = _listPicker;
			// add/remove slots from control to match element
			while (picker.Items.Count < Element.Items.Count + 1)
				picker.Items.Add(new ItemViewModel(string.Empty));

			while (picker.Items.Count > Element.Items.Count + 1)
				picker.Items.RemoveAt(picker.Items.Count - 1);

			// update all control values to match element values
			for (var i = 0; i < Element.Items.Count; i++)
			{
				var item = (ItemViewModel)picker.Items[i + 1];
				if (item.Data == Element.Items[i])
					continue;

				item.Data = Element.Items[i];
			}

			picker.SelectedIndex = Element.SelectedIndex + 1;

			_isChanging = false;
		}

		void UpdatePicker()
		{
			_listPicker.FullModeHeader = Element.Title;
			UpdateItems();
			_listPicker.SelectedIndex = Element.SelectedIndex + 1;
		}

		void UpdateTextColor()
		{
			if (!_listPicker.IsEnabled)
			{
				return;
			}

			Color color = Element.TextColor;
			_listPicker.Foreground = color.IsDefault ? (_defaultBrush ?? color.ToBrush()) : color.ToBrush();
		}

		class ItemViewModel : INotifyPropertyChanged
		{
			string _data;
			int _maxHeight;
			float _opacity;

			public ItemViewModel(string item)
			{
				_opacity = 1;
				_data = item;
				_maxHeight = int.MaxValue;
			}

			public string Data
			{
				get { return _data; }
				set
				{
					if (value == _data)
						return;

					_data = value;
					PropertyChanged(this, new PropertyChangedEventArgs("Data"));
				}
			}

			public int MaxHeight
			{
				get { return _maxHeight; }
				set
				{
					if (value == _maxHeight)
						return;

					_maxHeight = value;
					PropertyChanged(this, new PropertyChangedEventArgs("MaxHeight"));
				}
			}

			public float Opacity
			{
				get { return _opacity; }
				set
				{
					if (value == _opacity)
						return;

					_opacity = value;
					PropertyChanged(this, new PropertyChangedEventArgs("Opacity"));
				}
			}

			public event PropertyChangedEventHandler PropertyChanged = delegate { };
		}
	}
}