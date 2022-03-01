using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests
{
	public class DatePickerTextTransformCases : IEnumerable<object[]>
	{
		private readonly List<object[]> _data = new()
		{
			new object[] { new DateTime(2022, 01, 01), "dd MMMM yyyy", TextTransform.None, "01 January 2022" },
			new object[] { new DateTime(2022, 01, 01), "dd MMMM yyyy", TextTransform.Uppercase, "01 JANUARY 2022" },
			new object[] { new DateTime(2022, 01, 01), "dd MMMM yyyy", TextTransform.Lowercase, "01 january 2022" }
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
