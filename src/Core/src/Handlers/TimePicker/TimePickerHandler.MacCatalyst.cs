using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : ViewHandler<ITimePicker, UIDatePicker>
	{
		readonly UIDatePickerProxy _proxy = new();

		protected override UIDatePicker CreatePlatformView()
		{
			return new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };
		}

		internal bool UpdateImmediately { get; set; } = true;

		protected override void ConnectHandler(UIDatePicker platformView)
		{
			base.ConnectHandler(platformView);

			_proxy.Connect(this, VirtualView, platformView);
		}

		protected override void DisconnectHandler(UIDatePicker platformView)
		{
			base.DisconnectHandler(platformView);

			_proxy.Disconnect(platformView);
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
			//handler.PlatformView?.UpdateCharacterSpacing(timePicker);
		}

		public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			//handler.PlatformView?.UpdateFont(timePicker, fontManager);
		}

		public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
		{
			//handler.PlatformView?.UpdateTextColor(timePicker, DefaultTextColor);
		}

		public static void MapFlowDirection(TimePickerHandler handler, ITimePicker timePicker)
		{
			// handler.PlatformView?.UpdateFlowDirection(timePicker);
			// handler.PlatformView?.UpdateTextAlignment(timePicker);
		}

		internal static void MapIsOpen(ITimePickerHandler handler, ITimePicker timePicker)
		{

		}

		void SetVirtualViewTime()
		{
			if (VirtualView == null || PlatformView == null)
				return;

			var datetime = PlatformView.Date.ToDateTime();
			VirtualView.Time = new TimeSpan(datetime.Hour, datetime.Minute, 0);
		}

		class UIDatePickerProxy
		{
			WeakReference<TimePickerHandler>? _handler;
			WeakReference<ITimePicker>? _virtualView;

			ITimePicker? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(TimePickerHandler handler, ITimePicker virtualView, UIDatePicker platformView)
			{
				_handler = new(handler);
				_virtualView = new(virtualView);

				platformView.EditingDidBegin += OnStarted;
				platformView.EditingDidEnd += OnEnded;
				platformView.ValueChanged += OnValueChanged;
			}

			public void Disconnect(UIDatePicker platformView)
			{
				_virtualView = null;

				platformView.EditingDidBegin -= OnStarted;
				platformView.EditingDidEnd -= OnEnded;
				platformView.ValueChanged -= OnValueChanged;
				platformView.RemoveFromSuperview();
			}

			void OnStarted(object? sender, EventArgs eventArgs)
			{
				if (VirtualView is ITimePicker virtualView)
					virtualView.IsFocused = true;
			}

			void OnEnded(object? sender, EventArgs eventArgs)
			{
				if (VirtualView is ITimePicker virtualView)
					virtualView.IsFocused = false;
			}

			void OnValueChanged(object? sender, EventArgs e)
			{
				if (_handler is not null && _handler.TryGetTarget(out var handler) && handler.UpdateImmediately)
				{
					handler.SetVirtualViewTime();
				}
			}
		}
	}
}