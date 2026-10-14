using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>A complete binary invocation request. Application headers are metadata, not HTTP headers.</summary>
	public sealed class HybridWebViewBinaryRequest
	{
		internal HybridWebViewBinaryRequest(string methodName, byte[] payload, IReadOnlyDictionary<string, string> headers)
		{
			MethodName = methodName;
			Payload = payload;
			Headers = headers;
		}

		/// <summary>Gets the command to approve and dispatch. No method lookup is performed by MAUI.</summary>
		public string MethodName { get; }

		/// <summary>Gets the complete, request-owned bytes, without JSON or text conversion.</summary>
		public byte[] Payload { get; }

		/// <summary>Gets a read-only copy of validated application metadata (at most 16 ASCII headers).</summary>
		public IReadOnlyDictionary<string, string> Headers { get; }
	}
}
