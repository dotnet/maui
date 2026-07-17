#nullable enable
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	// NOTE: The Android passkey implementation uses AndroidX Credential Manager
	// (Jetpack `androidx.credentials`), which requires the `Xamarin.AndroidX.Credentials`
	// binding NuGet package. That package is not yet available in the dotnet-public build
	// feeds, so it cannot be referenced without breaking restore for the whole repo.
	//
	// Until the binding is ingested into the feed, Android reports IsSupported = false and
	// throws. The full Credential Manager implementation is ready to drop in here (replacing
	// this stub, re-adding the PackageReference in Essentials.csproj + eng/AndroidX.targets)
	// once the dependency is available. Tracked as a follow-up on the Passkeys spec PR.
	partial class PasskeysImplementation : IPasskeys
	{
		public bool IsSupported => false;

		public Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default)
			=> throw new FeatureNotSupportedException("Passkeys on Android require the AndroidX Credential Manager binding, which is not yet available in this build. See the Passkeys spec PR for status.");

		public Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
			=> throw new FeatureNotSupportedException("Passkeys on Android require the AndroidX Credential Manager binding, which is not yet available in this build. See the Passkeys spec PR for status.");
	}
}
