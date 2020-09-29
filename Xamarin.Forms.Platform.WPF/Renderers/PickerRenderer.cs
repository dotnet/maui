using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xamarin.Forms.Internals;
using WSelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;

namespace Xamarin.Forms.Platform.WPF
{
	public class PickerRenderer : ViewRenderer<Picker, ComboBox>
	{
		const string TextBoxTemplate = "PART_EditableTextBox";
		bool _isDisposed;

		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new ComboBox());
					Control.IsEditable = true;
					Control.SelectionChanged += OnControlSelectionChanged;
					Control.Loaded += OnControlLoaded;
				}

				// Update control property 
				UpdateTitle();
				UpdateSelectedIndex();
				UpdateTextColor();
				UpdateHorizontalTextAlignment();
				UpdateVerticalTextAlignment();
				Control.ItemsSource = ((LockableObservableListWrapper)Element.Items)._list;
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Picker.TitleProperty.PropertyName)
			{
				UpdateTitle();
			}
			else if (e.PropertyName == Picker.SelectedIndexProperty.PropertyName)
			{
				UpdateSelectedIndex();
			}
			else if (e.PropertyName == Picker.TextColorProperty.PropertyName)
			{
				UpdateTextColor();
			}
			else if (e.PropertyName == Picker.HorizontalTextAlignmentProperty.PropertyName || e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateHorizontalTextAlignment();
			}
			else if (e.PropertyName == Picker.VerticalTextAlignmentProperty.PropertyName)
			{
				UpdateVerticalTextAlignment();
			}
			else if (e.PropertyName == Picker.BackgroundColorProperty.PropertyName)
			{
				UpdateBackgroundColor();
			}
		}

		void UpdateTitle()
		{
			//TODO: Create full size combobox
		}

		void UpdateTextColor()
		{
			Control.UpdateDependencyColor(ComboBox.ForegroundProperty, Element.TextColor);
		}

		void UpdateBackgroundColor()
		{
			var textbox = (TextBox)Control.Template.FindName(TextBoxTemplate, Control);
			if (textbox != null)
			{
				var parent = (Border)textbox.Parent;
				parent.Background = Element.BackgroundColor.ToBrush();
			}
		}

		void UpdateSelectedIndex()
		{
			Control.SelectedIndex = Element.SelectedIndex;
		}

		void UpdateHorizontalTextAlignment()
		{
			Control.HorizontalContentAlignment = Element.HorizontalTextAlignment.ToNativeHorizontalAlignment();
		}

		void UpdateVerticalTextAlignment()
		{
			Control.VerticalContentAlignment = Element.VerticalTextAlignment.ToNativeVerticalAlignment();
		}

		void OnControlSelectionChanged(object sender, WSelectionChangedEventArgs e)
		{
			if (Element != null)
				Element.SelectedIndex = Control.SelectedIndex;
		}

		void OnControlLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			UpdateBackgroundColor();
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.SelectionChanged -= OnControlSelectionChanged;
					Control.Loaded -= OnControlLoaded;
					Control.ItemsSource = null;
				}

				if (Element != null)
				{

				}
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}
