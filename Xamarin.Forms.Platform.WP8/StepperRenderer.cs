using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WButton = System.Windows.Controls.Button;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class StepperRenderer : ViewRenderer<Stepper, Border>
	{
		readonly StackPanel _panel = new StackPanel();
		WButton _downButton;
		WButton _upButton;

		IElementController ElementController => Element as IElementController;

		protected override void OnElementChanged(ElementChangedEventArgs<Stepper> e)
		{
			base.OnElementChanged(e);

			var border = new Border();
			border.Child = _panel;
			_panel.HorizontalAlignment = HorizontalAlignment.Right;
			_panel.Orientation = Orientation.Horizontal;

			_upButton = new WButton { Content = "+", Width = 100 };
			_downButton = new WButton { Content = "-", Width = 100 };

			_panel.Children.Add(_downButton);
			_panel.Children.Add(_upButton);

			SetNativeControl(border);

			_upButton.Click += UpButtonOnClick;
			_downButton.Click += DownButtonOnClick;

			UpdateButtons();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			switch (e.PropertyName)
			{
				case "Minimum":
				case "Maximum":
				case "Value":
					UpdateButtons();
					break;
			}
		}

		void DownButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			ElementController.SetValueFromRenderer(Stepper.ValueProperty, Math.Max(Element.Minimum, Element.Value - Element.Increment));
		}

		void UpButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			ElementController.SetValueFromRenderer(Stepper.ValueProperty, Math.Min(Element.Maximum, Element.Value + Element.Increment));
		}

		void UpdateButtons()
		{
			_upButton.IsEnabled = Element.Value < Element.Maximum;
			_downButton.IsEnabled = Element.Value > Element.Minimum;
		}
	}
}