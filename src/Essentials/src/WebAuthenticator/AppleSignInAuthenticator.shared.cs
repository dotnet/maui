#nullable enable
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	/// <summary>
	/// Represents platform native Apple Sign In authentication APIs.
	/// </summary>
	public interface IAppleSignInAuthenticator
	{
		/// <summary>
		/// Performs a native Apple Sign In authentication request.
		/// </summary>
		/// <param name="options">Additional Sign In options.</param>
		/// <returns>A <see cref="WebAuthenticatorResult"/> object with the results of this operation.</returns>
		Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options? options = null);
	}

	/// <summary>
	/// Represents platform native Apple Sign In authentication APIs.
	/// </summary>
	/// <remarks>This API is only supported on iOS 13.0+ and should not be called on other devices at runtime.</remarks>
	public static class AppleSignInAuthenticator
	{
		/// <summary>
		/// Performs a native Apple Sign In authentication request.
		/// </summary>
		/// <param name="options">Additional Sign In options.</param>
		/// <returns>A <see cref="WebAuthenticatorResult"/> object with the results of this operation.</returns>
		public static Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options? options = null)
			=> Default.AuthenticateAsync(options ?? new AppleSignInAuthenticator.Options());

		static IAppleSignInAuthenticator? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IAppleSignInAuthenticator Default =>
			defaultImplementation ??= new AppleSignInAuthenticatorImplementation();

		internal static void SetDefault(IAppleSignInAuthenticator? implementation) =>
			defaultImplementation = implementation;

		/// <summary>
		/// Represents additional options for Apple Sign In.
		/// </summary>
		public class Options
		{
			/// <summary>
			/// Gets or sets whether to request the full name scope during sign in.
			/// </summary>
			public bool IncludeFullNameScope { get; set; } = false;

			/// <summary>
			/// Gets or sets whether to request the email scope during sign in.
			/// </summary>
			public bool IncludeEmailScope { get; set; } = false;
		}
	}
}
