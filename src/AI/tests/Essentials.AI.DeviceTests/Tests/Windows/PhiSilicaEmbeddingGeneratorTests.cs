#if WINDOWS

using Microsoft.Extensions.AI;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.DeviceTests;

[Category("PhiSilicaEmbeddingGenerator")]
public class PhiSilicaEmbeddingGeneratorCancellationTests : EmbeddingGeneratorCancellationTestsBase<PhiSilicaEmbeddingGenerator>
{
}

[Category("PhiSilicaEmbeddingGenerator")]
public class PhiSilicaEmbeddingGeneratorConcurrencyTests : EmbeddingGeneratorConcurrencyTestsBase<PhiSilicaEmbeddingGenerator>
{
}

[Category("PhiSilicaEmbeddingGenerator")]
public class PhiSilicaEmbeddingGeneratorDisposalTests : EmbeddingGeneratorDisposalTestsBase<PhiSilicaEmbeddingGenerator>
{
	[Fact]
	public void Dispose_WithModel_DisposesSuccessfully()
	{
		var generator = new PhiSilicaEmbeddingGenerator();
		generator.Dispose();
		// No exception means success
	}
}

[Category("PhiSilicaEmbeddingGenerator")]
public class PhiSilicaEmbeddingGeneratorGenerateTests : EmbeddingGeneratorGenerateTestsBase<PhiSilicaEmbeddingGenerator>
{
}

[Category("PhiSilicaEmbeddingGenerator")]
public class PhiSilicaEmbeddingGeneratorGetServiceTests : EmbeddingGeneratorGetServiceTestsBase<PhiSilicaEmbeddingGenerator>
{
	protected override string ExpectedProviderName => "windows";
	protected override string ExpectedDefaultModelId => "phi-silica";
}

[Category("PhiSilicaEmbeddingGenerator")]
public class PhiSilicaEmbeddingGeneratorInstantiationTests : EmbeddingGeneratorInstantiationTestsBase<PhiSilicaEmbeddingGenerator>
{
}

[Category("PhiSilicaEmbeddingGenerator")]
public class PhiSilicaEmbeddingGeneratorSimilarityTests : EmbeddingGeneratorSimilarityTestsBase<PhiSilicaEmbeddingGenerator>
{
}

#endif
