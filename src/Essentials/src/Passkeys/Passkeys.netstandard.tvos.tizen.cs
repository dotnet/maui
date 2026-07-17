#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	partial class PasskeysImplementation : IPasskeys
	{
		public bool IsSupported => false;

		public Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;

		public Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
			=> throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
