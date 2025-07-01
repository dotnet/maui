using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Utilities for creating HttpClient instances with proper SSL certificate handling.
	/// Based on fix from dotnet/android for certificate revocation check failures.
	/// </summary>
	internal static class HttpClientUtilities
	{
		/// <summary>
		/// Creates an HttpClient with proper SSL certificate revocation handling to avoid
		/// intermittent failures when certificate revocation lists are not available.
		/// </summary>
		/// <returns>HttpClient configured to handle SSL certificate revocation issues</returns>
		public static HttpClient CreateHttpClient()
		{
			// Originally from: https://github.com/dotnet/arcade/pull/15546
			// Configure the cert revocation check in a fail-open state to avoid intermittent failures
			// on Mac if the endpoint is not available. This is only available on .NET Core, but has only been
			// observed on Mac anyway.

			var handler = new SocketsHttpHandler();
			handler.SslOptions.CertificateChainPolicy = new X509ChainPolicy
			{
				// Yes, check revocation.
				// Yes, allow it to be downloaded if needed.
				// Online is the default, but it doesn't hurt to be explicit.
				RevocationMode = X509RevocationMode.Online,
				// Roots never bother with revocation.
				// ExcludeRoot is the default, but it doesn't hurt to be explicit.
				RevocationFlag = X509RevocationFlag.ExcludeRoot,
				// RevocationStatusUnknown at the EndEntity/Leaf certificate will not fail the chain build.
				// RevocationStatusUnknown for any intermediate CA will not fail the chain build.
				// IgnoreRootRevocationUnknown could also be specified, but it won't apply given ExcludeRoot above.
				// The default is that all status codes are bad, this is not the default.
				VerificationFlags =
					X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown |
					X509VerificationFlags.IgnoreEndRevocationUnknown,
				// Always use the "now" when building the chain, rather than the "now" of when this policy object was constructed.
				VerificationTimeIgnored = true,
			};

			return new HttpClient(handler);
		}
	}
}