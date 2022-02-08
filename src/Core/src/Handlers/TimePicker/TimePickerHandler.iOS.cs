using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		static UIColor? DefaultTextColor;

		protected override MauiTimePicker CreatePlatformView()
		{
			return new MauiTimePicker(() =>
			{
				SetVirtualViewTime();
				PlatformView?.ResignFirstResponder();
			});
		}

		protected override void ConnectHandler(MauiTimePicker platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView != null)
			{
				platformView.EditingDidBegin += OnStarted;
				platformView.EditingDidEnd += OnEnded;
				platformView.ValueChanged += OnValueChanged;
			}
		}

		protected override void DisconnectHandler(MauiTimePicker platformView)
		{
			base.DisconnectHandler(platformView);

			if (platformView != null)
			{
				platformView.RemoveFromSuperview();
				platformView.EditingDidBegin -= OnStarted;
				platformView.EditingDidEnd -= OnEnded;
				platformView.ValueChanged -= OnValueChanged;
				platformView.Dispose();
			}
		}

		void SetupDefaults(MauiTimePicker platformView)
		{
			DefaultTextColor = platformView.TextColor;
		}

		public static void MapFormat(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateFormat(timePicker);
		}

		public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTime(timePicker);
		}

		public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(TimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTextColor(timePicker, DefaultTextColor);
		}

		void OnStarted(object? sender, EventArgs eventArgs)
		{
			// TODO: Update IsFocused property
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			// TODO: Update IsFocused property
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			SetVirtualViewTime();
		}

		void SetVirtualViewTime()
		{
			if (VirtualView == null || PlatformView == null)
				return;

			VirtualView.Time = PlatformView.Date.ToDateTime() - new DateTime(1, 1, 1);
		}
	}
}