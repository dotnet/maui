using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WPFCheckBox = System.Windows.Controls.CheckBox;
using WControl = System.Windows.Controls.Control;
using System.Windows.Media;

namespace Xamarin.Forms.Platform.WPF
{
	public class CheckBoxRenderer : ViewRenderer<CheckBox, FormsCheckBox>
	{
		bool _isDisposed;
		static Brush _tintDefaultBrush = Color.Transparent.ToBrush();

		public CheckBoxRenderer()
		{
			
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null) // construct and SetNativeControl and suscribe control event
				{
					SetNativeControl(new FormsCheckBox()
					{
						Style = (System.Windows.Style)System.Windows.Application.Current.MainWindow.FindResource("FormsCheckBoxStyle")
					});

					Control.Checked += OnNativeChecked;
					Control.Unchecked += OnNativeChecked;
				}

				// Update control property 
				UpdateIsChecked();
				UpdateTintColor();
			}

			base.OnElementChanged(e);
		}
		
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == CheckBox.IsCheckedProperty.PropertyName)
			{
				UpdateIsChecked();
			}
			else if (e.PropertyName == CheckBox.ColorProperty.PropertyName)
			{
				UpdateTintColor();
			}
		}
		
		void UpdateTintColor()
		{
			if (Element.Color == Color.Default)
				Control.TintBrush = _tintDefaultBrush;
			else
				Control.TintBrush = Element.Color.ToBrush();

		}

		void UpdateIsChecked()
		{
			Control.IsChecked = Element.IsChecked;
		}

		void OnNativeChecked(object sender, System.Windows.RoutedEventArgs e)
		{
			((IElementController)Element).SetValueFromRenderer(CheckBox.IsCheckedProperty, Control.IsChecked);
		}
		
		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;
			_isDisposed = true;

			if (disposing && Control != null)
			{
				Control.Checked -= OnNativeChecked;
				Control.Unchecked -= OnNativeChecked;
			}

			base.Dispose(disposing);
		}
	}
}