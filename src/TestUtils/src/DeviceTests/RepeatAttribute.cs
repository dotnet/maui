using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>	
	/// Usage Example
	/// [Theory]
	/// [Repeat(100)]
	/// public async Task TheSameImageSourceReturnsTheSameBitmap(int _)
	/// </summary>
	public sealed class RepeatAttribute : DataAttribute
	{
		readonly int _count;

		public RepeatAttribute(int count)
		{
			this._count = count;
		}

		public override bool SupportsDiscoveryEnumeration() => true;

		public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
			MethodInfo testMethod, DisposalTracker disposalTracker) =>
			new(Enumerable.Range(1, _count)
				.Select(iteration => (ITheoryDataRow)new TheoryDataRow(iteration)).ToArray());
	}
}
