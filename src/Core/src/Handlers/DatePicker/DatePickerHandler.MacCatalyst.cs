using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class DatePickerHandler : ViewHandler<IDatePicker, UIDatePicker>
	{
		readonly UIDatePickerProxy _proxy = new();
		NSObject? _windowCloseObserver;
		readonly List<UITextField> _textFields = new();
		bool _isOpen;

		static readonly NSString NSWindowDidCloseNotification = new("NSWindowDidCloseNotification");

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
			FindAndWireTextFields(platformView);

			// On MacCatalyst the popover runs in an AppKit NSWindow. Tapping outside
			// dismisses it at the AppKit level without firing UITextField EditingDidEnd,
			// so NSWindowDidCloseNotification is needed for close.
			_windowCloseObserver = NSNotificationCenter.DefaultCenter.AddObserver(NSWindowDidCloseNotification, OnWindowClosed);

			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(UIDatePicker platformView)
		{
			_proxy.Disconnect(platformView);

			UnwireTextFields();

			if (_windowCloseObserver is not null)
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver(_windowCloseObserver);
				_windowCloseObserver = null;
			}

			_isOpen = false;

			base.DisconnectHandler(platformView);
		}

		void FindAndWireTextFields(UIView view)
		{
			foreach (var subview in view.Subviews)
			{
				if (subview is UITextField textField)
				{
					textField.EditingDidBegin += OnEditingDidBegin;
					_textFields.Add(textField);
				}
				else
				{
					FindAndWireTextFields(subview);
				}
			}
		}

		void UnwireTextFields()
		{
			foreach (var textField in _textFields)
			{
				textField.EditingDidBegin -= OnEditingDidBegin;
			}
			_textFields.Clear();
		}

		void OnEditingDidBegin(object? sender, EventArgs e)
		{
			if (_isOpen)
			{
				return;
			}

			_isOpen = true;
			if (VirtualView is IDatePicker virtualView)
			{
				virtualView.IsFocused = virtualView.IsOpen = true;
			}
		}

		void OnWindowClosed(NSNotification notification)
		{
			if (!_isOpen)
			{
				return;
			}

			_isOpen = false;

			if (UpdateImmediately)
			{
				SetVirtualViewDate();
			}

			if (VirtualView is IDatePicker virtualView)
			{
				virtualView.IsFocused = virtualView.IsOpen = false;
			}
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

		public static partial void MapFlowDirection(DatePickerHandler handler, IDatePicker datePicker)
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
			}
		}
	}
}
