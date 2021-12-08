using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>	
	/// Usage Example
	/// [Theory]
	/// [Repeat(100)]
	/// public async Task TheSameImageSourceReturnsTheSameBitmap(int _)
	/// </summary>
	public sealed class RepeatAttribute : Xunit.Sdk.DataAttribute
	{
		private readonly int count;

		public RepeatAttribute(int count)
		{
			if (count < 1)
			{
				throw new System.ArgumentOutOfRangeException(
					paramName: nameof(count),
					message: "Repeat count must be greater than 0."
					);
			}
			this.count = count;
		}

		public override System.Collections.Generic.IEnumerable<object[]> GetData(System.Reflection.MethodInfo testMethod)
		{
			foreach (var iterationNumber in Enumerable.Range(start: 1, count: this.count))
			{
				yield return new object[] { iterationNumber };
			}
		}
	}
}
