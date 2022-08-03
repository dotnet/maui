#nullable enable
using System.Threading.Tasks;

namespace Microsoft.Maui.Authentication
{
	public interface IAppleSignInAuthenticator
	{
		Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options? options = null);
	}

	/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="Type[@FullName='Microsoft.Maui.Essentials.AppleSignInAuthenticator']/Docs" />
	public static class AppleSignInAuthenticator
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/AppleSignInAuthenticator.xml" path="//Member[@MemberName='AuthenticateAsync']/Docs" />
		public static Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options? options = null)
			=> Default.AuthenticateAsync(options ?? new AppleSignInAuthenticator.Options());

		static IAppleSignInAuthenticator? defaultImplementation;

		public static IAppleSignInAuthenticator Default =>
			defaultImplementation ??= new AppleSignInAuthenticatorImplementation();

		internal static void SetDefault(IAppleSignInAuthenticator? implementation) =>
			defaultImplementation = implementation;

		public class Options
		{
			public bool IncludeFullNameScope { get; set; } = false;

			public bool IncludeEmailScope { get; set; } = false;
		}
	}
}
