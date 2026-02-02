using Microsoft.Extensions.AI;
using Microsoft.Windows.AI.Text;
using System.Runtime.Versioning;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Provides an <see cref="IEmbeddingGenerator{TInput, TEmbedding}"/> implementation using Windows Copilot Runtime
/// (Phi Silica) for generating text embeddings.
/// </summary>
/// <remarks>
/// <para>
/// This generator uses the Windows Copilot Runtime <see cref="LanguageModel"/> API to generate vector embeddings for text.
/// The <see cref="LanguageModel"/> is marked as agile (MarshalingBehaviorAttribute with MarshalingType.Agile and 
/// ThreadingAttribute with ThreadingModel.Both), which means it can be safely accessed from multiple threads 
/// concurrently without additional synchronization.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows10.0.26100.0")]
public sealed class PhiSilicaEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
	/// <summary>Lazily-initialized task that creates the underlying <see cref="LanguageModel"/>.</summary>
	private Task<LanguageModel> _modelTask;

	/// <summary>Whether this instance owns the <see cref="LanguageModel"/> and is responsible for disposing it.</summary>
	private readonly bool _ownsModel;

	/// <summary>Metadata about the embedding generator.</summary>
	private EmbeddingGeneratorMetadata? _metadata;

	/// <summary>
	/// Initializes a new instance of the <see cref="PhiSilicaEmbeddingGenerator"/> class.
	/// </summary>
	/// <remarks>
	/// The generator will create a <see cref="LanguageModel"/> and reuse it for all requests.
	/// </remarks>
	public PhiSilicaEmbeddingGenerator()
	{
		_modelTask = PhiSilicaModelFactory.CreateModelAsync();
		_ownsModel = true;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PhiSilicaEmbeddingGenerator"/> class
	/// with the specified <see cref="LanguageModel"/>.
	/// </summary>
	/// <param name="model">The <see cref="LanguageModel"/> to use for generating embeddings.</param>
	/// <remarks>
	/// When using this constructor, the generator does not own the <see cref="LanguageModel"/>
	/// and will not dispose it. The caller is responsible for disposing the model.
	/// </remarks>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="model"/> is <see langword="null"/>.</exception>
	public PhiSilicaEmbeddingGenerator(LanguageModel model)
	{
		ArgumentNullException.ThrowIfNull(model);
		_modelTask = Task.FromResult(model);
		_ownsModel = false;
	}

	/// <inheritdoc />
	public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
		IEnumerable<string> values,
		EmbeddingGenerationOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(values);
		if (!values.Any())
		{
			throw new ArgumentException("The values collection must contain at least one value.", nameof(values));
		}

		var potentialCount = values.TryGetNonEnumeratedCount(out var count) ? count : 0;

		var result = new GeneratedEmbeddings<Embedding<float>>(potentialCount);

		// Get the model (already created in constructor or provided)
		var model = await _modelTask;

		foreach (var value in values)
		{
			ArgumentException.ThrowIfNullOrEmpty(value);

			cancellationToken.ThrowIfCancellationRequested();

			// Generate embedding vectors for the input text (synchronous API)
			var embeddingResult = model.GenerateEmbeddingVectors(value);

			// Extract the first embedding vector (should only be one for a single input)
			if (embeddingResult.EmbeddingVectors.Count > 0)
			{
				var embeddingVector = embeddingResult.EmbeddingVectors[0];
				var dimension = (int)embeddingVector.Size;

				// Pre-allocate array and extract values
				var vectorData = new float[dimension];
				embeddingVector.GetValues(vectorData);

				result.Add(new Embedding<float>(vectorData));
			}
			else
			{
				// If no vectors returned, add an empty embedding
				result.Add(new Embedding<float>(Array.Empty<float>()));
			}
		}

		return result;
	}

	/// <inheritdoc />
	object? IEmbeddingGenerator.GetService(Type serviceType, object? serviceKey)
	{
		ArgumentNullException.ThrowIfNull(serviceType);

		if (serviceKey is not null)
		{
			return null;
		}

		if (serviceType == typeof(EmbeddingGeneratorMetadata))
		{
			return _metadata ??= new EmbeddingGeneratorMetadata(
				providerName: "windows",
				defaultModelId: "phi-silica");
		}

		if (serviceType == typeof(LanguageModel))
		{
			// Return the model if it's already created
			return _modelTask.IsCompletedSuccessfully ? _modelTask.Result : null;
		}

		if (serviceType.IsInstanceOfType(this))
		{
			return this;
		}

		return null;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		// If the task completed successfully, dispose the model
		if (_ownsModel && _modelTask.IsCompletedSuccessfully)
		{
			_modelTask.Result.Dispose();
		}
	}
}
