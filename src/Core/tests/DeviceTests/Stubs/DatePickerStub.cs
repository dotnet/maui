using System;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class DatePickerStub : StubBase, IDatePicker
	{
		public string Format { get; set; }

		public DateTime Date { get; set; }

		public DateTime MinimumDate { get; set; }

		public DateTime MaximumDate { get; set; }
	}
}