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
				platformView.UpdateTime(VirtualView.Time);
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

		public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateFormat(timePicker);
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTime(timePicker);
		}

		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTextColor(timePicker, DefaultTextColor);
		}

		void OnStarted(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = true;
		}

		void OnEnded(object? sender, EventArgs eventArgs)
		{
			if (VirtualView != null)
				VirtualView.IsFocused = false;
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			SetVirtualViewTime();
		}

		void SetVirtualViewTime()
		{
			if (VirtualView == null || PlatformView == null)
				return;

			var datetime = PlatformView.Date.ToDateTime();
			VirtualView.Time = new TimeSpan(datetime.Hour, datetime.Minute, 0);
		}
	}
}