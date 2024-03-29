﻿using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class SliderHandler : ViewHandler<ISlider, UISlider>
	{
		readonly SliderProxy _proxy = new();

		protected override UISlider CreatePlatformView()
		{
			var platformSlider = new UISlider { Continuous = true };

			if (OperatingSystem.IsMacCatalystVersionAtLeast(15) && platformSlider.TraitCollection.UserInterfaceIdiom == UIUserInterfaceIdiom.Mac)
				platformSlider.PreferredBehavioralStyle = UIBehavioralStyle.Pad;

			return platformSlider;
		}

		protected override void ConnectHandler(UISlider platformView)
		{
			base.ConnectHandler(platformView);
			_proxy.Connect(VirtualView, platformView);
		}

		protected override void DisconnectHandler(UISlider platformView)
		{
			base.DisconnectHandler(platformView);
			_proxy.Disconnect(platformView);
		}

		public static void MapMinimum(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMinimum(slider);
		}

		public static void MapMaximum(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMaximum(slider);
		}

		public static void MapValue(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateValue(slider);
		}

		public static void MapMinimumTrackColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMinimumTrackColor(slider);
		}

		public static void MapMaximumTrackColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateMaximumTrackColor(slider);
		}

		public static void MapThumbColor(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateThumbColor(slider);
		}

		public static void MapThumbImageSource(ISliderHandler handler, ISlider slider)
		{
			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			handler.PlatformView?.UpdateThumbImageSourceAsync(slider, provider)
				.FireAndForget(handler);
		}

		class SliderProxy
		{
			WeakReference<ISlider>? _virtualView;

			ISlider? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(ISlider virtualView, UISlider platformView)
			{
				_virtualView = new(virtualView);
				platformView.ValueChanged += OnControlValueChanged;
				platformView.AddTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
				platformView.AddTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);
			}

			public void Disconnect(UISlider platformView)
			{
				platformView.ValueChanged -= OnControlValueChanged;
				platformView.RemoveTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
				platformView.RemoveTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);
			}

			void OnControlValueChanged(object? sender, EventArgs eventArgs)
			{
				if (VirtualView is ISlider virtualView && sender is UISlider platformView)
					virtualView.Value = platformView.Value;
			}

			void OnTouchDownControlEvent(object? sender, EventArgs e)
			{
				VirtualView?.DragStarted();
			}

			void OnTouchUpControlEvent(object? sender, EventArgs e)
			{
				VirtualView?.DragCompleted();
			}
		}
	}
}