using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public class FlyoutPageLayoutBehaviorTestCases : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			yield return new object[] { typeof(FlyoutPage) };
			yield return new object[] { typeof(FlyoutPageAlwaysSplit) };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
