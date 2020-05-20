using System;
using System.Collections.Generic;
using System.Text;
using WDatePicker = System.Windows.Controls.DatePicker;


namespace System.Maui.Platform
{
	public partial class DatePickerRenderer : AbstractViewRenderer<IDatePicker, WDatePicker>
	{
		protected override WDatePicker CreateView()
		{
			var control = new WDatePicker();
			control.SelectedDateChanged += OnSelectedDateChanged;
			return control;
		}

		public static void MapPropertyMaximumDate(IViewRenderer renderer, IDatePicker datePicker) => (renderer as DatePickerRenderer)?.UpdateMaximumDate();
		public static void MapPropertyMinimumDate(IViewRenderer renderer, IDatePicker datePicker) => (renderer as DatePickerRenderer)?.UpdateMinimumDate();

		void OnSelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (TypedNativeView.SelectedDate.HasValue)
				VirtualView.SelectedDate = TypedNativeView.SelectedDate.Value;
		}

		void UpdateDate()
		{
			TypedNativeView.SelectedDate = VirtualView.SelectedDate;
		}

		void UpdateMaximumDate()
		{
			TypedNativeView.DisplayDateEnd = VirtualView.MaximumDate;
		}

		void UpdateMinimumDate()
		{
			TypedNativeView.DisplayDateStart = VirtualView.MinimumDate;
		}
	}
}
