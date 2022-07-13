using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public class ShellFlyoutHeaderBehaviorTestCases : IEnumerable<object[]>
	{
		public IEnumerator<object[]> GetEnumerator()
		{
			foreach (var behavior in Enum.GetValues(typeof(FlyoutHeaderBehavior)))
				yield return new object[] { (FlyoutHeaderBehavior)behavior };
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
