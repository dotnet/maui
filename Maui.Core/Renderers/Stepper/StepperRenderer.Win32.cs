using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WButton = System.Windows.Controls.Button;

namespace System.Maui.Platform
{
	public partial class StepperRenderer : AbstractViewRenderer<IStepper, Border>
	{
		readonly StackPanel _panel = new StackPanel();
		WButton _downButton;
		WButton _upButton;

		protected override Border CreateView()
		{
			var control = CreateControl();

			_upButton.Click += UpButtonOnClick;
			_downButton.Click += DownButtonOnClick;

			return control;
		}

		protected override void DisposeView(Border border)
		{
			_upButton.Click -= UpButtonOnClick;
			_downButton.Click -= DownButtonOnClick;

			base.DisposeView(border);
		}

		public static void MapPropertyMinimum(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateButtons();
		public static void MapPropertyMaximum(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateButtons();
		public static void MapPropertyIncrement(IViewRenderer renderer, IStepper slider) { }
		public static void MapPropertyValue(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateButtons();
		public static void MapPropertyIsEnabled(IViewRenderer renderer, IStepper slider) => (renderer as StepperRenderer)?.UpdateEnabled();

		public virtual void UpdateEnabled()
		{
			_panel.IsEnabled = VirtualView.IsEnabled;
		}

		public virtual void UpdateButtons()
		{
			var value = VirtualView.Value;
			_upButton.IsEnabled = value < VirtualView.Maximum;
			_downButton.IsEnabled = value > VirtualView.Minimum;
		}

		Border CreateControl()
		{
			var border = new Border() { Child = _panel };
			_panel.HorizontalAlignment = HorizontalAlignment.Right;
			_panel.Orientation = System.Windows.Controls.Orientation.Horizontal;

			_upButton = new WButton { Content = "+", Width = 100 };
			_downButton = new WButton { Content = "-", Width = 100 };

			_panel.Children.Add(_downButton);
			_panel.Children.Add(_upButton);
			return border;
		}

		void DownButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			VirtualView.Value = Math.Max(VirtualView.Minimum, VirtualView.Value - VirtualView.Increment);
		}

		void UpButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
		{
			VirtualView.Value = Math.Min(VirtualView.Maximum, VirtualView.Value + VirtualView.Increment);
		}
	}
}