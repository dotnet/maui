using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class DatePickerStub : StubBase, IDatePicker
	{
		public string Format { get; set; }

		public DateTime? Date { get; set; }

		public DateTime? MinimumDate { get; set; }

		public DateTime? MaximumDate { get; set; }

		public double CharacterSpacing { get; set; }

		public Font Font { get; set; }

		public Color TextColor { get; set; }

		public bool IsOpen { get; set; }

		public void OnIsOpenPropertyChanged(bool oldValue, bool newValue)
		{

		}
	}
}