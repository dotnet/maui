using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Maui
{
	/// <summary>
	/// Explicitly approves and dispatches whole-buffer binary requests from the local web document.
	/// This invoker is independent of JSON invocation and never discovers methods using reflection.
	/// </summary>
	/// <remarks>
	/// Configure <see cref="IHybridWebViewBinary.BinaryInvoker"/> before loading the document, then call
	/// <c>await window.HybridWebView.InvokeDotNetBinary(methodName, arrayBufferOrView, { headers, signal })</c>
	/// from its top-level local page. The promise returns an <c>ArrayBuffer</c> or rejects with a structured failure.
	/// Only Apple, Android, and Windows handlers provide this transport. Apple and Windows upload raw bytes;
	/// Android uploads bounded encoded fragments before a single atomic dispatch. All return raw binary bytes.
	/// Four invocations may be active per view, with a 30-second deadline and 4 MiB input/output limits.
	/// Cancellation is cooperative: an ignored cancellation keeps its native invocation slot until completion.
	/// The host may be called on a background thread and must dispatch UI operations itself.
	/// Local same-origin code is trusted; do not serve untrusted HTML or a remote guest under the app origin.
	/// </remarks>
	public abstract class HybridWebViewBinaryInvoker
	{
		/// <summary>Initializes a binary invoker.</summary>
		protected HybridWebViewBinaryInvoker()
		{
		}

		/// <summary>
		/// Approves the command and returns its whole binary result. The host must validate command permissions,
		/// observe cancellation, and return no more than 4 MiB. Requests also have a 4 MiB limit.
		/// </summary>
		/// <param name="request">The validated command, application metadata, and complete payload.</param>
		/// <param name="cancellationToken">Cancellation on abort, timeout, document navigation, or disconnection.</param>
		/// <returns>The complete result, including an empty array for an empty result.</returns>
		public abstract Task<byte[]> InvokeAsync(HybridWebViewBinaryRequest request, CancellationToken cancellationToken);
	}
}
