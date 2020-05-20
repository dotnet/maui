using System.Maui.Core.Controls;
using AppKit;
using Foundation;

namespace System.Maui.Platform
{
	public partial class SliderRenderer : AbstractViewRenderer<ISlider, MauiSlider>
	{
		protected override MauiSlider CreateView()
		{
			var slider = new MauiSlider
			{
				Action = new ObjCRuntime.Selector(nameof(ValueChanged))
			};

			return slider;
		}

		protected override void DisposeView(MauiSlider slider)
		{
			slider.Action = null;

			base.DisposeView(slider);
		}

		public static void MapPropertyMinimum(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is MauiSlider mauiSlider))
				return;

			mauiSlider.MinValue = (float)slider.Minimum;
		}

		public static void MapPropertyMaximum(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is MauiSlider mauiSlider))
				return;

			mauiSlider.MaxValue = (float)slider.Maximum;
		}

		public static void MapPropertyValue(IViewRenderer renderer, ISlider slider)
		{
			if (!(renderer.NativeView is MauiSlider mauiSlider))
				return;

			if (Math.Abs(slider.Value - mauiSlider.DoubleValue) > 0)
				mauiSlider.DoubleValue = (float)slider.Value;
		}

		public static void MapPropertyMinimumTrackColor(IViewRenderer renderer, ISlider slider)
		{
			
		}

		public static void MapPropertyMaximumTrackColor(IViewRenderer renderer, ISlider slider)
		{
			
		}

		public static void MapPropertyThumbColor(IViewRenderer renderer, ISlider slider)
		{
			
		}

		[Export(nameof(ValueChanged))]
		void ValueChanged()
		{
			VirtualView.Value = TypedNativeView.DoubleValue;

			var controlEvent = NSApplication.SharedApplication.CurrentEvent;

			if (controlEvent.Type == NSEventType.LeftMouseDown)
				VirtualView.DragStarted();
			else if (controlEvent.Type == NSEventType.LeftMouseUp)
				VirtualView.DragCompleted();
		}
	}
}