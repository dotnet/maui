using System;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, UIView>
	{
		MauiTimePicker? GetMauiTimePicker() => PlatformView as MauiTimePicker;
		static MauiTimePicker? GetMauiTimePicker(ITimePickerHandler handler) => handler.PlatformView as MauiTimePicker;

		protected override UIView CreatePlatformView()
		{
			return new MauiTimePicker(() =>
			{
				SetVirtualViewTime();
				PlatformView?.ResignFirstResponder();
			});
		}

		protected override void ConnectHandler(UIView platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView is MauiTimePicker picker)
			{
				picker.EditingDidBegin += OnStarted;
				picker.EditingDidEnd += OnEnded;
				picker.ValueChanged += OnValueChanged;
				picker.DateSelected += OnDateSelected;
				picker.UpdateTime(VirtualView.Time);
			}
		}

		protected override void DisconnectHandler(UIView platformView)
		{
			base.DisconnectHandler(platformView);

			if (platformView is MauiTimePicker picker)
			{
				platformView.RemoveFromSuperview();

				picker.EditingDidBegin -= OnStarted;
				picker.EditingDidEnd -= OnEnded;
				picker.ValueChanged -= OnValueChanged;
				picker.DateSelected -= OnDateSelected;

				platformView.Dispose();
			}
		}

		public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
		{
			GetMauiTimePicker(handler)?.UpdateFormat(timePicker, GetMauiTimePicker(handler)?.Picker);
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
		{
			GetMauiTimePicker(handler)?.UpdateTime(timePicker, GetMauiTimePicker(handler)?.Picker);
		}

		public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker)
		{
			GetMauiTimePicker(handler)?.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			GetMauiTimePicker(handler)?.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			GetMauiTimePicker(handler)?.UpdateTextColor(timePicker);
		}

		public static void MapFlowDirection(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateFlowDirection(timePicker);
			GetMauiTimePicker(handler)?.UpdateTextAlignment(timePicker);
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

		void OnDateSelected(object? sender, EventArgs e)
		{
			SetVirtualViewTime();
		}

		void SetVirtualViewTime()
		{
			if (VirtualView == null || PlatformView == null)
				return;

			if (PlatformView is MauiTimePicker timePicker)
			{
				var datetime = timePicker.Date.ToDateTime();
				VirtualView.Time = new TimeSpan(datetime.Hour, datetime.Minute, 0);
			}
		}
	}
}