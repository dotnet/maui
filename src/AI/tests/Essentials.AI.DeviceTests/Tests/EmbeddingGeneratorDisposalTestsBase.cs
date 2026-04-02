using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for embedding generator disposal tests.
/// </summary>
/// <typeparam name="T">The embedding generator type to test.</typeparam>
public abstract class EmbeddingGeneratorDisposalTestsBase<T>
	where T : class, IEmbeddingGenerator<string, Embedding<float>>, IDisposable, new()
{
	[Fact]
	public void Dispose_CanBeCalledMultipleTimes()
	{
		var generator = new T();

		// Should not throw
		generator.Dispose();
		generator.Dispose();
	}
}
