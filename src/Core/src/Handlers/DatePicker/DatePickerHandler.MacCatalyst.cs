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

		bool _syncingOpenState;
		bool _nativeOpenState;

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
			if (handler is DatePickerHandler datePickerHandler)
				datePickerHandler.SyncOpenState(datePicker.IsOpen, refreshIfAlreadyFirstResponder: datePicker.IsOpen);
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

			var didOpen = datePickerHandler.SyncOpenState(shouldBeOpen: true, refreshIfAlreadyFirstResponder: !datePicker.IsOpen);
			request.TrySetResult(didOpen);
		}

		static void MapMacCatalystUnfocus(IViewHandler handler, IView view, object? args)
		{
			if (handler is not DatePickerHandler datePickerHandler)
				return;

			datePickerHandler.SyncOpenState(shouldBeOpen: false, refreshIfAlreadyFirstResponder: false);
		}

		bool SyncOpenState(bool shouldBeOpen, bool refreshIfAlreadyFirstResponder)
		{
			if (_syncingOpenState)
				return _nativeOpenState || PlatformView.IsFirstResponder;

			_syncingOpenState = true;
			try
			{
				var platformView = PlatformView;
				bool actualOpen;
				if (shouldBeOpen)
				{
					if (platformView.IsFirstResponder && refreshIfAlreadyFirstResponder)
						platformView.ResignFirstResponder();

					var didBecomeFirstResponder = platformView.IsFirstResponder;
					if (!platformView.IsFirstResponder)
						didBecomeFirstResponder = platformView.BecomeFirstResponder();

					// Stock UIDatePicker on MacCatalyst does not reliably report first-responder
					// state from a programmatic open request; custom platform views use native results.
					var canAssumeStockPickerOpened = platformView.GetType() == typeof(UIDatePicker) &&
						platformView.Enabled &&
						platformView.UserInteractionEnabled;

					actualOpen = platformView.IsFirstResponder || didBecomeFirstResponder || canAssumeStockPickerOpened;
				}
				else
				{
					if (platformView.IsFirstResponder || _nativeOpenState)
						platformView.ResignFirstResponder();

					actualOpen = platformView.IsFirstResponder;
				}

				SetVirtualOpenState(actualOpen);
				return actualOpen;
			}
			finally
			{
				_syncingOpenState = false;
			}
		}

		void SetVirtualOpenStateFromNative(bool isOpen)
		{
			if (_syncingOpenState)
				return;

			_syncingOpenState = true;
			try
			{
				SetVirtualOpenState(isOpen);
			}
			finally
			{
				_syncingOpenState = false;
			}
		}

		void SetVirtualOpenState(bool isOpen)
		{
			_nativeOpenState = isOpen;

			if (VirtualView is IDatePicker virtualView)
			{
				virtualView.IsOpen = isOpen;
				virtualView.IsFocused = isOpen;
			}
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

			public void Connect(DatePickerHandler handler, IDatePicker virtualView, UIDatePicker platformView)
			{
				_handler = new(handler);

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
				if (_handler is not null && _handler.TryGetTarget(out var handler))
				{
					if (handler.UpdateImmediately)
						handler.SetVirtualViewDate();

					handler.SetVirtualOpenStateFromNative(true);
				}
			}

			void OnStarted(object? sender, EventArgs eventArgs)
			{
				if (_handler is not null && _handler.TryGetTarget(out var handler))
					handler.SetVirtualOpenStateFromNative(true);
			}

			void OnEnded(object? sender, EventArgs eventArgs)
			{
				if (_handler is not null && _handler.TryGetTarget(out var handler))
					handler.SetVirtualOpenStateFromNative(false);
			}
		}
	}
}
