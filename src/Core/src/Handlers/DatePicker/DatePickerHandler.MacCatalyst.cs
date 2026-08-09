using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, UIDatePicker>
	{
		static readonly NSString WindowDidCloseNotification = new("NSWindowDidCloseNotification");

		readonly UIDatePickerProxy _proxy = new();
		NSObject? _windowCloseObserver;
		bool _isDatePickerOpen;

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

			// The compact UIDatePicker on MacCatalyst uses internal UITextField subviews
			// for the date segments. Wire up EditingDidBegin directly on those fields.
			WireTextFields(platformView);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIDatePicker platformView)
		{
			_proxy.Disconnect(platformView);

			UnwireTextFields(platformView);

			if (_windowCloseObserver is not null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_windowCloseObserver);
				_windowCloseObserver = null;
			}

			_isDatePickerOpen = false;

			base.DisconnectHandler(platformView);
		}


		// Recursively traverses the view hierarchy and yields all UITextField subviews.
		// Used to find the internal text fields of the compact UIDatePicker on MacCatalyst.
		static IEnumerable<UITextField> GetTextFields(UIView view)
		{
			foreach (var subview in view.Subviews)
			{

				if (subview is UITextField textField)
				{
					yield return textField;
				}
				else
				{
					foreach (var nested in GetTextFields(subview))
					{
						yield return nested;
					}
				}
			}
		}

		void WireTextFields(UIView view)
		{
			foreach (var textField in GetTextFields(view))
			{
				textField.EditingDidBegin += OnEditingDidBegin;
			}
		}

		void UnwireTextFields(UIView view)
		{
			foreach (var textField in GetTextFields(view))
			{
				textField.EditingDidBegin -= OnEditingDidBegin;
			}
		}

		void OnEditingDidBegin(object? sender, EventArgs e)
		{
			if (_isDatePickerOpen)
			{
				return;
			}

			_isDatePickerOpen = true;

			// Register a one-shot observer scoped to this picker's open lifetime.
			// On MacCatalyst the popover runs in an AppKit NSWindow; tapping outside
			// dismisses it at the AppKit level without firing UITextField EditingDidEnd.
			// Registering here (not in ConnectHandler) avoids spurious fires from
			// unrelated window closes while the picker is not open.
			_windowCloseObserver = NSNotificationCenter.DefaultCenter.AddObserver(WindowDidCloseNotification, OnWindowClosed);

			SetVirtualOpenStateFromNative(true);
		}

		void OnWindowClosed(NSNotification notification)
		{
			// One-shot: remove the observer immediately so it won't fire again.
			if (_windowCloseObserver is not null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_windowCloseObserver);
				_windowCloseObserver = null;
			}

			_isDatePickerOpen = false;

			SetVirtualOpenStateFromNative(false);

			// On MacCatalyst the internal UITextFields stay as first responder
			// (visually highlighted) even after the popover window closes.
			// EndEditing(true) on the parent view does not propagate to them,
			// so we must directly resign each tracked text field on the next
			// run-loop iteration (the notification fires before UIKit is ready).
			ResignTextFields();
		}

		void ResignTextFields()
		{
			var platformView = PlatformView;
			CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
			{
				foreach (var textField in GetTextFields(platformView))
				{
					if (textField.IsFirstResponder)
					{
						textField.ResignFirstResponder();
					}
				}
			});
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

				platformView.ValueChanged += OnValueChanged;
			}

			public void Disconnect(UIDatePicker platformView)
			{
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
		}
	}
}
