using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class OneTimeInitializationActionTests
	{
		[Fact]
		public void ConcurrentCallersInitializeOnce()
		{
			var invocationCount = 0;
			var initialization = new OneTimeInitializationAction(
				() => Interlocked.Increment(ref invocationCount));

			Parallel.For(0, 32, _ => initialization.InvokeOnce());

			Assert.Equal(1, invocationCount);
		}
	}
}
