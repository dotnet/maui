using System;

namespace Microsoft.Maui.Handlers
{
#if IOS && !MACCATALYST
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		protected override MauiTimePicker CreatePlatformView()
		{
			var timePicker = new MauiTimePicker(() =>
			{
			});

			timePicker.DateSelected += (_, _) =>
			{
				SetVirtualViewTime();
				PlatformView?.ResignFirstResponder();
			};

			return timePicker;
		}

		internal bool UpdateImmediately { get; set; }

		protected override void ConnectHandler(MauiTimePicker platformView)
		{
			base.ConnectHandler(platformView);

			if (platformView != null)
			{
				platformView.EditingDidBegin += OnStarted;
				platformView.EditingDidEnd += OnEnded;
				platformView.ValueChanged += OnValueChanged;
				platformView.DateSelected += OnDateSelected;
				platformView.PickerValueChanged += OnValueChanged;

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
				platformView.DateSelected -= OnDateSelected;
				platformView.PickerValueChanged -= OnValueChanged;

				platformView.Dispose();
			}
		}

		public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateFormat(timePicker, handler.PlatformView?.Picker);
		}

		public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateTime(timePicker, handler.PlatformView?.Picker);
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
			handler.PlatformView?.UpdateTextColor(timePicker);
		}

		public static void MapFlowDirection(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateFlowDirection(timePicker);
			handler.PlatformView?.UpdateTextAlignment(timePicker);
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
			if (UpdateImmediately)  // Platform Specific
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

			var datetime = PlatformView.Date.ToDateTime();
			VirtualView.Time = new TimeSpan(datetime.Hour, datetime.Minute, 0);
		}
	}
#endif
}