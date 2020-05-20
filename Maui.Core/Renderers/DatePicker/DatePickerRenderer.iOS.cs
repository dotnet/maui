using System.Drawing;
using Foundation;
using UIKit;

namespace System.Maui.Platform
{
	public partial class DatePickerRenderer : AbstractViewRenderer<IDatePicker, PickerView>
	{
		UIColor _defaultTextColor;
		UIDatePicker _datePicker;

		protected override PickerView CreateView()
		{
			var pickerView = new PickerView();
			_defaultTextColor = pickerView.TextColor;

			_datePicker = new UIDatePicker
			{
				Mode = UIDatePickerMode.Date,
				TimeZone = new NSTimeZone("UTC"),
				Date = VirtualView.SelectedDate.ToNSDate()
			};

			var width = (float)UIScreen.MainScreen.Bounds.Width;
			var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
			{
				VirtualView.SelectedDate = _datePicker.Date.ToDateTime();
				pickerView.ResignFirstResponder();
			});

			toolbar.SetItems(new[] { spacer, doneButton }, false);

			_datePicker.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			toolbar.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			pickerView.SetInputView(_datePicker);
			pickerView.SetInputAccessoryView(toolbar);

			return pickerView;
		}

		protected override void DisposeView(PickerView nativeView)
		{
			_defaultTextColor = null;
			_datePicker = null;
			nativeView.SetInputView(null);
			nativeView.SetInputAccessoryView(null);
			base.DisposeView(nativeView);
		}

		public static void MapPropertyMaximumDate(IViewRenderer renderer, IDatePicker datePicker)
		{
			(renderer as DatePickerRenderer)?.UpdateMaximumDate();
		}
		
		public static void MapPropertyMinimumDate(IViewRenderer renderer, IDatePicker datePicker)
		{
			(renderer as DatePickerRenderer)?.UpdateMinimumDate();
		}


		protected virtual void UpdateMaximumDate()
		{
			if (_datePicker == null)
			{
				return;
			}

			_datePicker.MaximumDate = VirtualView.MaximumDate.ToNSDate();
		}
		
		protected virtual void UpdateMinimumDate()
		{
			if (_datePicker == null)
			{
				return;
			}

			_datePicker.MinimumDate = VirtualView.MinimumDate.ToNSDate();
		}
	}
}
