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

			if (VirtualView is IDatePicker virtualView)
			{
				virtualView.IsFocused = virtualView.IsOpen = true;
			}
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

			if (VirtualView is IDatePicker virtualView)
			{
				virtualView.IsFocused = virtualView.IsOpen = false;
			}

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

				platformView.ValueChanged += OnValueChanged;
			}

			public void Disconnect(UIDatePicker platformView)
			{
				platformView.ValueChanged -= OnValueChanged;
			}

			void OnValueChanged(object? sender, EventArgs? e)
			{
				if (_handler is not null && _handler.TryGetTarget(out var handler) && handler.UpdateImmediately)
					handler.SetVirtualViewDate();

				if (VirtualView is IDatePicker virtualView)
					virtualView.IsFocused = true;
			}
		}
	}
}
