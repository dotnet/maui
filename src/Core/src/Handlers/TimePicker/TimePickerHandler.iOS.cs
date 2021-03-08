using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Handlers
{
	public partial class TimePickerHandler : AbstractViewHandler<ITimePicker, MauiTimePicker>
	{
		MauiTimePicker? _nativeTimePicker;
		static UIDatePicker? Picker;

		protected override MauiTimePicker CreateNativeView()
		{
			_nativeTimePicker = new MauiTimePicker();

			Picker = new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };

			if (NativeVersion.IsAtLeast(14))
			{
				Picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			var width = UIScreen.MainScreen.Bounds.Width;
			var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
			{
				SetVirtualViewTime();
				_nativeTimePicker.ResignFirstResponder();
			});

			toolbar.SetItems(new[] { spacer, doneButton }, false);

			_nativeTimePicker.InputView = Picker;
			_nativeTimePicker.InputAccessoryView = toolbar;

			_nativeTimePicker.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			_nativeTimePicker.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			_nativeTimePicker.InputAssistantItem.LeadingBarButtonGroups = null;
			_nativeTimePicker.InputAssistantItem.TrailingBarButtonGroups = null;

			_nativeTimePicker.AccessibilityTraits = UIAccessibilityTrait.Button;

			return _nativeTimePicker;
		}

		protected override void ConnectHandler(MauiTimePicker nativeView)
		{
			if (Picker != null)
				Picker.ValueChanged += OnValueChanged;
		}

		protected override void DisconnectHandler(MauiTimePicker nativeView)
		{
			if (Picker != null)
			{
				Picker.RemoveFromSuperview();
				Picker.ValueChanged -= OnValueChanged;
				Picker.Dispose();
				Picker = null;
			}
		}

		public static void MapFormat(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.TypedNativeView?.UpdateFormat(timePicker, Picker);
		}

		public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
		{
			handler.TypedNativeView?.UpdateTime(timePicker, Picker);
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			SetVirtualViewTime();
		}

		void SetVirtualViewTime()
		{
			if (VirtualView == null || Picker == null)
				return;

			VirtualView.Time = Picker.Date.ToDateTime() - new DateTime(1, 1, 1);
		}
	}
}