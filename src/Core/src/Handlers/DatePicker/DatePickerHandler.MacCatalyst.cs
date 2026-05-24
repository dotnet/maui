using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, UIDatePicker>
	{
		readonly UIDatePickerProxy _proxy = new();

		static DatePickerHandler()
		{
			// IDatePicker does not implement IPicker, so Focus/Unfocus overrides must
			// run through an IView mapper instead of DatePickerHandler.CommandMapper entries.
			var macCatalystOverrides = new CommandMapper<IView, IViewHandler>(CommandMapper.Chained ?? ViewCommandMapper)
			{
				[nameof(IView.Focus)] = MapMacCatalystFocus,
				[nameof(IView.Unfocus)] = MapMacCatalystUnfocus,
			};

			CommandMapper.Chained = macCatalystOverrides;
		}

		protected override UIDatePicker CreatePlatformView()
		{
			return new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };
		}

		internal bool UpdateImmediately { get; set; } = true;

		protected override void ConnectHandler(UIDatePicker platformView)
		{
			_proxy.Connect(this, VirtualView, platformView);

			var date = VirtualView?.Date;
			if (date is not null && date is DateTime dt)
			{
				platformView.Date = dt.ToNSDate();
			}

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIDatePicker platformView)
		{
			_proxy.Disconnect(platformView);

			base.DisconnectHandler(platformView);
		}

		public static partial void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateFormat(datePicker);
		}

		public static partial void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateDate(datePicker);
		}

		public static partial void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateMinimumDate(datePicker);
		}

		public static partial void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateMaximumDate(datePicker);
		}

		public static partial void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
		{
		}

		public static partial void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
		{

		}

		public static partial void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
		{

		}

		public static partial void MapFlowDirection(IDatePickerHandler handler, IDatePicker datePicker)
		{

		}

		internal static partial void MapIsOpen(IDatePickerHandler handler, IDatePicker datePicker)
		{
			handler.PlatformView?.UpdateIsOpen(datePicker);
		}

		static void MapMacCatalystFocus(IViewHandler handler, IView view, object? args)
		{
			if (args is not FocusRequest request)
				return;

			if (handler is not DatePickerHandler datePickerHandler ||
				datePickerHandler.VirtualView is not IDatePicker datePicker)
			{
				request.TrySetResult(false);
				return;
			}

			datePicker.IsOpen = true;
			datePickerHandler.PlatformView?.UpdateIsOpen(datePicker);

			// UIDatePicker can report false from BecomeFirstResponder on MacCatalyst.
			request.TrySetResult(true);
		}

		static void MapMacCatalystUnfocus(IViewHandler handler, IView view, object? args)
		{
			if (handler is not DatePickerHandler datePickerHandler ||
				datePickerHandler.VirtualView is not IDatePicker datePicker)
			{
				return;
			}

			datePicker.IsOpen = false;
			datePickerHandler.PlatformView?.UpdateIsOpen(datePicker);
		}

		void SetVirtualViewDate()
		{
			if (VirtualView is null)
			{
				return;
			}

			VirtualView.Date = PlatformView.Date.ToDateTime();
		}

		class UIDatePickerProxy
		{
			WeakReference<DatePickerHandler>? _handler;
			WeakReference<IDatePicker>? _virtualView;

			IDatePicker? VirtualView => _virtualView is not null && _virtualView.TryGetTarget(out var v) ? v : null;

			public void Connect(DatePickerHandler handler, IDatePicker virtualView, UIDatePicker platformView)
			{
				_handler = new(handler);
				_virtualView = new(virtualView);

				platformView.EditingDidBegin += OnStarted;
				platformView.EditingDidEnd += OnEnded;
				platformView.ValueChanged += OnValueChanged;
			}

			public void Disconnect(UIDatePicker platformView)
			{
				platformView.EditingDidBegin -= OnStarted;
				platformView.EditingDidEnd -= OnEnded;
				platformView.ValueChanged -= OnValueChanged;
			}

			void OnValueChanged(object? sender, EventArgs? e)
			{
				if (_handler is not null && _handler.TryGetTarget(out var handler) && handler.UpdateImmediately)
					handler.SetVirtualViewDate();

				if (VirtualView is IDatePicker virtualView)
					virtualView.IsFocused = virtualView.IsOpen = true;
			}

			void OnStarted(object? sender, EventArgs eventArgs)
			{
				if (VirtualView is IDatePicker virtualView)
					virtualView.IsFocused = virtualView.IsOpen = true;
			}

			void OnEnded(object? sender, EventArgs eventArgs)
			{
				if (VirtualView is IDatePicker virtualView)
					virtualView.IsFocused = virtualView.IsOpen = false;
			}
		}
	}
}
