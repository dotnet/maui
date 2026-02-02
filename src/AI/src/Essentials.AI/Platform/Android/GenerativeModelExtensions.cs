using System.Threading.Channels;
using Com.Microsoft.Maui.Essentials.AI;
using Google.MLKit.GenAI.Prompt;

namespace Microsoft.Maui.Essentials.AI;

/// <summary>
/// Extension methods for IGenerativeModel providing Task-based async APIs
/// </summary>
internal static class GenerativeModelExtensions
{
	public static Task<int> CheckStatusAsync(this IGenerativeModel model, CancellationToken cancellationToken = default)
	{
		var listener = new ModelStatusListener();
		var signal = GenerativeModelExtensionsKt.CheckStatus(model, listener);
		cancellationToken.Register(signal.Cancel);
		return listener.Task;
	}

	public static Task WarmupAsync(this IGenerativeModel model, CancellationToken cancellationToken = default)
	{
		var listener = new ModelWarmupListener();
		var signal = GenerativeModelExtensionsKt.Warmup(model, listener);
		cancellationToken.Register(signal.Cancel);
		return listener.Task;
	}

	public static Task<GenerateContentResponse> GenerateContentAsync(
		this IGenerativeModel model,
		GenerateContentRequest request,
		CancellationToken cancellationToken = default)
	{
		var listener = new ContentGenerationListener();
		var signal = GenerativeModelExtensionsKt.GenerateContent(model, request, listener);
		cancellationToken.Register(signal.Cancel);
		return listener.Task;
	}

	public static IAsyncEnumerable<GenerateContentResponse> GenerateContentStreamAsync(
		this IGenerativeModel model,
		GenerateContentRequest request,
		CancellationToken cancellationToken = default)
	{
		var listener = new StreamContentGenerationListener();
		var signal = GenerativeModelExtensionsKt.GenerateContentStream(model, request, listener);
		cancellationToken.Register(signal.Cancel);
		return listener.ReadAllAsync(cancellationToken);
	}

	// Listener implementations

	private sealed class ModelStatusListener : Java.Lang.Object, IModelStatusListener
	{
		private readonly TaskCompletionSource<int> _tcs = new();

		public Task<int> Task => _tcs.Task;

		public void OnSuccess(int status) => _tcs.TrySetResult(status);

		public void OnFailure(Java.Lang.Throwable error) => _tcs.TrySetException(error);
	}

	private sealed class ModelWarmupListener : Java.Lang.Object, IModelWarmupListener
	{
		private readonly TaskCompletionSource _tcs = new();

		public Task Task => _tcs.Task;

		public void OnSuccess() => _tcs.TrySetResult();

		public void OnFailure(Java.Lang.Throwable error) => _tcs.TrySetException(error);
	}

	private sealed class ContentGenerationListener : Java.Lang.Object, IContentGenerationListener
	{
		private readonly TaskCompletionSource<GenerateContentResponse> _tcs = new();

		public Task<GenerateContentResponse> Task => _tcs.Task;

		public void OnSuccess(GenerateContentResponse response) => _tcs.TrySetResult(response);

		public void OnFailure(Java.Lang.Throwable error) => _tcs.TrySetException(error);
	}

	private sealed class StreamContentGenerationListener : Java.Lang.Object, IStreamContentGenerationListener
	{
		private readonly Channel<GenerateContentResponse> _channel;
		private readonly ChannelWriter<GenerateContentResponse> _writer;
		private readonly ChannelReader<GenerateContentResponse> _reader;

		public StreamContentGenerationListener()
		{
			_channel = Channel.CreateUnbounded<GenerateContentResponse>();
			_writer = _channel.Writer;
			_reader = _channel.Reader;
		}

		public IAsyncEnumerable<GenerateContentResponse> ReadAllAsync(CancellationToken cancellationToken = default) =>
			_reader.ReadAllAsync(cancellationToken);

		public void OnResponse(GenerateContentResponse response) =>
			_writer.TryWrite(response);

		public void OnComplete(Java.Lang.Throwable? error) =>
			_writer.TryComplete(error);
	}
}
