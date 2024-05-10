using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
using Microsoft.Maui.ApplicationModel;
using UIKit;

namespace Microsoft.Maui.Authentication
{
	partial class AppleSignInAuthenticatorImplementation : IAppleSignInAuthenticator
	{
		AuthManager authManager;

		public async Task<WebAuthenticatorResult> AuthenticateAsync(AppleSignInAuthenticator.Options options)
		{
			// why, you might ask, is this going through a private method that does the implementation and
			// not in this method? The answer is that this the amount of work that gets done is apparently
			// complicated enough to confuse the compiler and trigger *many* CA1416 errors. Factoring like this
			// fixes those errors.
			if ((OperatingSystem.IsIOSVersionAtLeast(13, 0)) || (OperatingSystem.IsMacCatalystVersionAtLeast(13, 1)))
				return await AuthenticateAsyncImpl(options);
			throw new FeatureNotEnabledException();
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst13.1")]
		async Task<WebAuthenticatorResult> AuthenticateAsyncImpl(AppleSignInAuthenticator.Options options)
		{
			var provider = new ASAuthorizationAppleIdProvider();
			var req = provider.CreateRequest();

			authManager = new AuthManager(WindowStateManager.Default.GetCurrentUIWindow(true));

			var scopes = new List<ASAuthorizationScope>();

			if (options.IncludeFullNameScope)
				scopes.Add(ASAuthorizationScope.FullName);
			if (options.IncludeEmailScope)
				scopes.Add(ASAuthorizationScope.Email);

			req.RequestedScopes = scopes.ToArray();
			var controller = new ASAuthorizationController(new[] { req });

			controller.Delegate = authManager;
			controller.PresentationContextProvider = authManager;

			controller.PerformRequests();

			var creds = await authManager.GetCredentialsAsync();

			if (creds == null)
				return null;

			var idToken = new NSString(creds.IdentityToken, NSStringEncoding.UTF8).ToString();
			var authCode = new NSString(creds.AuthorizationCode, NSStringEncoding.UTF8).ToString();
			var name = NSPersonNameComponentsFormatter.GetLocalizedString(creds.FullName, NSPersonNameComponentsFormatterStyle.Default, 0);

			var appleAccount = new WebAuthenticatorResult();
			appleAccount.Properties.Add("id_token", idToken);
			appleAccount.Properties.Add("authorization_code", authCode);
			appleAccount.Properties.Add("state", creds.State);
			appleAccount.Properties.Add("email", creds.Email);
			appleAccount.Properties.Add("user_id", creds.User);
			appleAccount.Properties.Add("name", name);
			appleAccount.Properties.Add("realuserstatus", creds.RealUserStatus.ToString());

			return appleAccount;
		}
	}

	[System.Runtime.Versioning.SupportedOSPlatform("ios13.0")]
	[System.Runtime.Versioning.SupportedOSPlatform("maccatalyst13.1")]
	class AuthManager : NSObject, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
	{
		public Task<ASAuthorizationAppleIdCredential> GetCredentialsAsync()
			=> tcsCredential?.Task;

		TaskCompletionSource<ASAuthorizationAppleIdCredential> tcsCredential;

		UIWindow presentingAnchor;

		public AuthManager(UIWindow presentingWindow)
		{
			tcsCredential = new TaskCompletionSource<ASAuthorizationAppleIdCredential>();
			presentingAnchor = presentingWindow;
		}

		public UIWindow GetPresentationAnchor(ASAuthorizationController controller)
			=> presentingAnchor;

		[Export("authorizationController:didCompleteWithAuthorization:")]
		public void DidComplete(ASAuthorizationController controller, ASAuthorization authorization)
		{
			var creds = authorization.GetCredential<ASAuthorizationAppleIdCredential>();
			tcsCredential?.TrySetResult(creds);
		}

		[Export("authorizationController:didCompleteWithError:")]
		public void DidComplete(ASAuthorizationController controller, NSError error)
			=> tcsCredential?.TrySetException(new Exception(error.LocalizedDescription));
	}
}
