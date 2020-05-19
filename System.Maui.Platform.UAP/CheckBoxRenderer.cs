using System.ComponentModel;
using global::Windows.UI.Xaml;
using global::Windows.UI.Xaml.Media;

namespace System.Maui.Platform.UWP
{
	public class CheckBoxRenderer : ViewRenderer<CheckBox, FormsCheckBox>
	{
		static Brush _tintDefaultBrush = Color.Blue.ToBrush();
		bool _disposed = false;

		protected virtual FormsCheckBox CreateNativeControl()
		{
			return new FormsCheckBox()
			{
				Style = global::Windows.UI.Xaml.Application.Current.Resources["FormsCheckBoxStyle"] as global::Windows.UI.Xaml.Style,			
			};
		}

		protected override void OnElementChanged(ElementChangedEventArgs<CheckBox> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var control = CreateNativeControl();
					control.Checked += OnNativeChecked;
					control.Unchecked += OnNativeChecked;

					SetNativeControl(control);
				}

				UpdateIsChecked();
				UpdateFlowDirection();
				UpdateTintColor();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (Control != null && disposing)
			{
				Control.Checked -= OnNativeChecked;
				Control.Unchecked -= OnNativeChecked;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == CheckBox.IsCheckedProperty.PropertyName)
			{
				UpdateIsChecked();
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection();
			}
			else if(e.PropertyName == CheckBox.ColorProperty.PropertyName)
			{
				UpdateTintColor();
			}
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnNativeChecked(object sender, RoutedEventArgs routedEventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(CheckBox.IsCheckedProperty, Control.IsChecked);
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}


		void UpdateIsChecked()
		{
			Control.IsChecked = Element.IsChecked;
		}

		void UpdateTintColor()
		{
			BrushHelpers.UpdateColor(Element.Color, ref _tintDefaultBrush,
				() => Control.TintBrush, brush => Control.TintBrush = brush);
			
		}
	}
}