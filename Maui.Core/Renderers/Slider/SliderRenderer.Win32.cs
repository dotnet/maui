using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WSlider = System.Windows.Controls.Slider;

namespace System.Maui.Platform
{
	public partial class SliderRenderer : AbstractViewRenderer<ISlider, WSlider>
	{
		protected override WSlider CreateView()
		{
			var slider = new WSlider();
			slider.ValueChanged += OnSliderValueChanged;
			return slider;
		}

		protected override void DisposeView(WSlider nativeView)
		{
			nativeView.ValueChanged -= OnSliderValueChanged;
			base.DisposeView(nativeView);
		}

		public static void MapPropertyMinimum(IViewRenderer renderer, ISlider slider) => (renderer as SliderRenderer)?.UpdateMinimum();
		public static void MapPropertyMaximum(IViewRenderer renderer, ISlider slider) => (renderer as SliderRenderer)?.UpdateMaximum();
		public static void MapPropertyValue(IViewRenderer renderer, ISlider slider) => (renderer as SliderRenderer)?.UpdateValue();
		public static void MapPropertyMinimumTrackColor(IViewRenderer renderer, ISlider slider) { }
		public static void MapPropertyMaximumTrackColor(IViewRenderer renderer, ISlider slider) { }
		public static void MapPropertyThumbColor(IViewRenderer renderer, ISlider slider) { }

		public virtual void UpdateMinimum()
		{
			TypedNativeView.Minimum = VirtualView.Minimum;
		}

		public virtual void UpdateMaximum()
		{
			TypedNativeView.Maximum = VirtualView.Maximum;
		}

		public virtual void UpdateValue()
		{
			var newValue = VirtualView.Value;
			if (TypedNativeView.Value != newValue)
				TypedNativeView.Value = newValue;
		}

		void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (TypedNativeView.Value != VirtualView.Value)
				VirtualView.Value = TypedNativeView.Value;
		}
	}
}