using System;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	internal static partial class AssertionExtensions
	{
		public static void AssertHasFlag(this Enum self, Enum flag)
		{
			var hasFlag = self.HasFlag(flag);

			if (!hasFlag)
				throw new ContainsException(flag, self);
		}
	}
}