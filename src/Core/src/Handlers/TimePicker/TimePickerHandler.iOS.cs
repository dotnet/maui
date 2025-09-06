using System;

namespace Microsoft.Maui.Handlers
{
#if IOS && !MACCATALYST
	public partial class TimePickerHandler : ViewHandler<ITimePicker, MauiTimePicker>
	{
		readonly MauiTimePickerProxy _proxy = new();

		protected override MauiTimePicker CreatePlatformView()
		{
			return new MauiTimePicker(_proxy.OnDateSelected);
		}

		internal bool UpdateImmediately { get; set; }

		protected override void ConnectHandler(MauiTimePicker platformView)
		{
			base.ConnectHandler(platformView);

			_proxy.Connect(this, VirtualView, platformView);
			platformView.UpdateTime(VirtualView.Time);
		}

		protected override void DisconnectHandler(MauiTimePicker platformView)
		{
			base.DisconnectHandler(platformView);

			_proxy.Disconnect(platformView);
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

		internal static void MapIsOpen(ITimePickerHandler handler, ITimePicker timePicker)
		{
			handler.PlatformView?.UpdateIsOpen(timePicker);
		}

		void SetVirtualViewTime()
		{
			if (VirtualView == null || PlatformView == null)
				return;

			var datetime = PlatformView.Date.ToDateTime();
			VirtualView.Time = new TimeSpan(datetime.Hour, datetime.Minute, 0);
		}

		class MauiTimePickerProxy
		{
			WeakReference<TimePickerHandler>? _handler;
			WeakReference<ITimePicker>? _virtualView;

			ITimePicker? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(TimePickerHandler handler, ITimePicker virtualView, MauiTimePicker platformView)
			{
				_handler = new(handler);
				_virtualView = new(virtualView);

				platformView.EditingDidBegin += OnStarted;
				platformView.EditingDidEnd += OnEnded;
				platformView.ValueChanged += OnValueChanged;
				platformView.Picker.ValueChanged += OnValueChanged;
			}

			public void Disconnect(MauiTimePicker platformView)
			{
				_virtualView = null;

				platformView.EditingDidBegin -= OnStarted;
				platformView.EditingDidEnd -= OnEnded;
				platformView.ValueChanged -= OnValueChanged;
				platformView.Picker.ValueChanged -= OnValueChanged;
				platformView.RemoveFromSuperview();
			}

			void OnStarted(object? sender, EventArgs eventArgs)
			{
				VirtualView?.IsFocused = VirtualView.IsOpen = true;
			}

			void OnEnded(object? sender, EventArgs eventArgs)
			{
				VirtualView?.IsFocused = VirtualView.IsOpen = false;
			}

			void OnValueChanged(object? sender, EventArgs e)
			{
				if (_handler is not null && _handler.TryGetTarget(out var handler) && handler.UpdateImmediately)  // Platform Specific
					handler.SetVirtualViewTime();
			}

			public void OnDateSelected()
			{
				if (_handler is not null && _handler.TryGetTarget(out var handler))
				{
					handler.SetVirtualViewTime();
					handler.PlatformView?.ResignFirstResponder();
				}
			}
		}
	}
#endif
}