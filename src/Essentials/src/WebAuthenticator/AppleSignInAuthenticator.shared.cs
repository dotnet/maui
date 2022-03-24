#nullable enable
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	public interface IAppleSignInAuthenticator
	{
		Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticatorOptions? options = null);
	}

	public class AppleSignInAuthenticatorOptions
	{
		public bool IncludeFullNameScope { get; set; } = false;

		public bool IncludeEmailScope { get; set; } = false;
	}

	public static partial class AppleSignInAuthenticator
	{
		static IAppleSignInAuthenticator? defaultImplementation;

		public static IAppleSignInAuthenticator Default =>
			defaultImplementation ??= new AppleSignInAuthenticatorImplementation();

		internal static void SetDefault(IAppleSignInAuthenticator? implementation) =>
			defaultImplementation = implementation;
	}
}
