using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests
{
	public class TimePickerTextTransformCases : IEnumerable<object[]>
	{
		readonly List<object[]> _data = new()
		{
			new object[] { new TimeSpan(4, 15, 26), "T", TextTransform.None, "4:15:26 AM" },
			new object[] { new TimeSpan(4, 15, 26), "T", TextTransform.Uppercase, "4:15:26 AM" },
			new object[] { new TimeSpan(4, 15, 26), "T", TextTransform.Lowercase, "4:15:26 am" }
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
