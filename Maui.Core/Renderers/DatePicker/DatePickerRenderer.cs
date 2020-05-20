using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui.Platform
{
	public partial class DatePickerRenderer
	{
		public static PropertyMapper<IDatePicker> DatePickerMapper = new PropertyMapper<IDatePicker>(LabelRenderer.ITextMapper)
		{
#if __MACOS__
			[nameof(IDatePicker.SelectedDate)] = MapPropertySelectedDate,		
#else
			[nameof(IDatePicker.SelectedDate)] = LabelRenderer.MapPropertyText,
#endif
			[nameof(IDatePicker.MaximumDate)] = MapPropertyMaximumDate,
			[nameof(IDatePicker.MinimumDate)] = MapPropertyMinimumDate,
		};

		public DatePickerRenderer() : base(DatePickerMapper)
		{

		}

		public DatePickerRenderer(PropertyMapper mapper) : base(mapper)
		{
		}
		
	}
}
