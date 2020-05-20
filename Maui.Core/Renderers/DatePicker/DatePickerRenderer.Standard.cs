using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui.Platform
{
	public partial class DatePickerRenderer : AbstractViewRenderer<IDatePicker, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyMaximumDate(IViewRenderer renderer, IDatePicker datePicker) { }
		public static void MapPropertyMinimumDate(IViewRenderer renderer, IDatePicker datePicker) { }
	}
}
