using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiDatePicker : NoCaretField
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
		public MauiDatePicker()
		{
			BorderStyle = UITextBorderStyle.RoundedRect;

			var picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

			if (OperatingSystem.IsIOSVersionAtLeast(13, 4))
			{
				picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			picker.EditingDidBegin += (sender, e) =>
			{
				if (sender != null)
					_weakEventManager.HandleEvent(sender, e, nameof(InputViewEditingDidBegin));
			};

			picker.ValueChanged += (sender, e) =>
			{
				if (sender != null)
					_weakEventManager.HandleEvent(sender, e, nameof(InputViewValueChanged));
			};

			picker.EditingDidEnd += (sender, e) =>
			{
				if (sender != null)
					_weakEventManager.HandleEvent(sender, e, nameof(InputViewEditingDidEnd));
			};

			InputView = picker;
		}

		internal event EventHandler InputViewEditingDidBegin
		{
			add => _weakEventManager.AddEventHandler(value, nameof(InputViewEditingDidBegin));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(InputViewEditingDidBegin));
		}

		internal event EventHandler InputViewValueChanged
		{
			add => _weakEventManager.AddEventHandler(value, nameof(InputViewValueChanged));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(InputViewValueChanged));
		}

		internal event EventHandler InputViewEditingDidEnd
		{
			add => _weakEventManager.AddEventHandler(value, nameof(InputViewEditingDidEnd));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(InputViewEditingDidEnd));
		}
	}
}