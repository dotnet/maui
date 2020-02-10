using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AuthenticationServices;
using Foundation;
using UIKit;

namespace Xamarin.Essentials
{
    public static partial class AppleSignInAuthenticator
    {
        static AuthManager authManager;

        static async Task<WebAuthenticatorResult> PlatformAuthenticateAsync(Options options)
        {
            if (DeviceInfo.Version.Major < 13)
                throw new FeatureNotSupportedException();

            var provider = new ASAuthorizationAppleIdProvider();
            var req = provider.CreateRequest();

            authManager = new AuthManager(Platform.GetCurrentWindow());

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

            var creds = await authManager.Credentials;

            if (creds == null)
                return null;

            var appleAccount = new WebAuthenticatorResult();
            appleAccount.Properties.Add("id_token", new NSString(creds.IdentityToken, NSStringEncoding.UTF8).ToString());
            appleAccount.Properties.Add("email", creds.Email);
            appleAccount.Properties.Add("user_id", creds.User);
            appleAccount.Properties.Add("name", NSPersonNameComponentsFormatter.GetLocalizedString(creds.FullName, NSPersonNameComponentsFormatterStyle.Default, NSPersonNameComponentsFormatterOptions.Phonetic));
            appleAccount.Properties.Add("realuserstatus", creds.RealUserStatus.ToString());

            return appleAccount;
        }
    }

    class AuthManager : NSObject, IASAuthorizationControllerDelegate, IASAuthorizationControllerPresentationContextProviding
    {
        public Task<ASAuthorizationAppleIdCredential> Credentials
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
