namespace Microsoft.Maui
{
	/// <summary>
	/// Optional whole-buffer binary invocation capability for a local <see cref="IHybridWebView"/>.
	/// </summary>
	public interface IHybridWebViewBinary : IHybridWebView
	{
		/// <summary>
		/// Gets the explicitly approved binary invoker, or <see langword="null"/> to disable binary invocation.
		/// Changing this value invalidates pending invocations; reload the document to use the new configuration.
		/// </summary>
		HybridWebViewBinaryInvoker? BinaryInvoker { get; }
	}
}
