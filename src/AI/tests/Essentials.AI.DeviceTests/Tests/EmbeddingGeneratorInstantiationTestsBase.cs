using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

/// <summary>
/// Base class for embedding generator instantiation tests.
/// </summary>
/// <typeparam name="T">The embedding generator type to test.</typeparam>
public abstract class EmbeddingGeneratorInstantiationTestsBase<T>
	where T : class, IEmbeddingGenerator<string, Embedding<float>>, new()
{
	[Fact]
	public void DefaultConstructor_CreatesInstance()
	{
		var generator = new T();
		Assert.NotNull(generator);
	}

	[Fact]
	public void MultipleInstances_CanBeCreated()
	{
		var generator1 = new T();
		var generator2 = new T();

		Assert.NotNull(generator1);
		Assert.NotNull(generator2);
		Assert.NotSame(generator1, generator2);
	}
}
