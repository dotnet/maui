using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public class PickerRenderer : ViewRenderer<Picker, ComboBox>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Picker> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new ComboBox());
					Control.SelectionChanged += OnControlSelectionChanged;
				}

				// Update control property 
				UpdateTitle();
				UpdateSelectedIndex();
				UpdateTextColor();
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
		}

		void UpdateTitle()
		{
			//TODO: Create full size combobox
		}

		void UpdateTextColor()
		{
			Control.UpdateDependencyColor(ComboBox.ForegroundProperty, Element.TextColor);
		}

		void UpdateSelectedIndex()
		{
			Control.SelectedIndex = Element.SelectedIndex;
		}

		private void OnControlSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Element != null)
				Element.SelectedIndex = Control.SelectedIndex;
		}

		bool _isDisposed;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (Control != null)
				{
					Control.SelectionChanged -= OnControlSelectionChanged;
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
